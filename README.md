# Exemplo de Azure Function em C# que repete as mensagens enviadas pelo Whatsapp

## Preparação do ambiente

Para o exemplo funcionar é necessário ter uma conta no Whatsapp Business. Siga o [passo a passo](https://developers.facebook.com/docs/whatsapp/cloud-api/get-started) para configurar sua conta e um projeto.

Também é necessário criar uma Azure Function e subir o código do exemplo conforme mostrado [aqui](https://learn.microsoft.com/pt-br/azure/azure-functions/create-first-function-vs-code-csharp)

Este exemplo segue os mesmos princípios explicados nesse [tutorial para AWS](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/set-up-whatsapp-echo-bot). Para saber mais sobre webhooks acesse o [link](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/set-up-webhooks)

No site da Meta para desenvolvedores, abra seus [Apps](https://developers.facebook.com/apps) selecione o app que você criou e na guia "Configuração da API" gere um token "Gerar token de acesso".

Configure um webhook informando a URL da função no Azure Functions (por exemplo, https://meuapp.azurewebsites.net/api/HttpEcho) e informe uma palavra chave (ex.: minhachavesecreta) qualquer que será usada para validação. Ative o campo do tipo **messages** (Deve ficar marcado como Assinado)

No Azure Functions configure as seguintes variáveis de ambiente:

`VERIFY_TOKEN: minhachavesecreta` (ou outra palavra chave escolhida)

`WHATSAPP_TOKEN: EAA...xxx` (token gerado no portal do desenvolvedor)

Existem alguns testes no arquivo Tests.http
