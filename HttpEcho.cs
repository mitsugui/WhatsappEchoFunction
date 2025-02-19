using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Pensaai.Functions
{
    public class HttpEcho
    {
        private static readonly string? VERIFY_TOKEN = Environment.GetEnvironmentVariable("VERIFY_TOKEN"); // Store in app settings
        private static readonly string? WHATSAPP_TOKEN = Environment.GetEnvironmentVariable("WHATSAPP_TOKEN"); // Store in app settings
        private static readonly HttpClient _httpClient = new HttpClient(); // Create a single HttpClient instance

        private readonly ILogger<HttpEcho> _logger;

        public HttpEcho(ILogger<HttpEcho> logger)
        {
            _logger = logger;
        }

        [Function("HttpEcho")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (req.Method == "GET")
            {
                if (req.Query.TryGetValue("hub.mode", out var mode) && mode == "subscribe"
                    && req.Query.TryGetValue("hub.verify_token", out var verifyToken) && verifyToken == VERIFY_TOKEN
                    && req.Query.TryGetValue("hub.challenge", out var challenge))
                {
                    return new OkObjectResult(int.Parse(challenge.ToString() ?? "0"));
                }
                else
                {
                    return new StatusCodeResult(403); // Use StatusCodeResult for simpler 403 responses
                }
            }
            else if (req.Method == "POST")
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic? body = JsonConvert.DeserializeObject(requestBody);

                if ((body?.entry) == null)
                {
                    return new OkResult(); // Return OK even if no messages are processed to avoid retries.
                }

                _logger.LogInformation($"Body obtido.");
                foreach (var entry in body.entry)
                {
                    _logger.LogInformation($"Entry: {entry}.");
                    foreach (var change in entry.changes)
                    {
                        _logger.LogInformation($"Change: {change}.");
                        var value = change.value;
                        if (value == null) continue;

                        var phoneNumberId = value?.metadata?.phone_number_id.ToString();
                        _logger.LogInformation($"PhoneNumberId: {phoneNumberId}.");

                        if ((value?.messages) == null) continue;

                        foreach (var message in value.messages)
                        {
                            if ((message?.type) != "text") continue;

                            var from = message.from.ToString();
                            var messageBody = message?.text?.body;
                            var replyMessage = $"Ack from Azure Function: {messageBody}";

                            _logger.LogInformation($"Phone Number: {phoneNumberId} Message: {from} | Body: {messageBody}.");

                            await SendReply(phoneNumberId, from, replyMessage, _logger); // Use await
                            return new OkResult(); // 200 OK with no body
                        }
                    }
                }
                return new OkResult(); // Return OK even if no messages are processed to avoid retries.
            }
            else
            {
                return new StatusCodeResult(403);
            }
        }

        private static async Task SendReply(string phoneNumberId, string to, string replyMessage, ILogger<HttpEcho> logger)
        {
            var json = new
            {
                messaging_product = "whatsapp",
                to = to,
                text = new { body = replyMessage }
            };

            var serializedData = JsonConvert.SerializeObject(json);
            var data = new StringContent(serializedData, Encoding.UTF8, "application/json");
            var path = $"/v22.0/{phoneNumberId}/messages?access_token={WHATSAPP_TOKEN}";
            var url = $"https://graph.facebook.com{path}";

            try
            {

                logger.LogInformation($"Url: {url}.");

                logger.LogInformation($"Data: {serializedData}.");

                var response = await _httpClient.PostAsync(url, data); // Use HttpClient and await
                if (!response.IsSuccessStatusCode)
                {
                    // Handle error logging or retry logic here
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error sending reply: {response.StatusCode} - {errorContent}"); // Log the error
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (log, retry, etc.)
                Console.WriteLine($"Exception sending reply: {ex.Message}");
            }
        }
    }
}
