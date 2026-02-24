using System.Net.Mail;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using NuvPizza.Domain.Interfaces;
using NuvPizza.Domain.Entities;
using MimeKit;

namespace NuvPizza.Infrastructure.Services;

public class GmailService : IEmailService
{
    private readonly string[] Scopes = { Google.Apis.Gmail.v1.GmailService.Scope.GmailSend };
    private readonly string ApplicationName = "NuvPizza API";
    private readonly string CredentialsFilePath = "credentials.json";
    private readonly string TokenDirectoryPath = "tokens";

    public async Task EnviarEmailConfirmacao(Pedido pedido)
    {
        if (string.IsNullOrWhiteSpace(pedido.EmailCliente))
        {
            // Cliente não informou e-mail no checkout
            return;
        }

        try
        {
            var service = await InitializeGmailServiceAsync();

            var message = CreateEmailMessage(pedido);

            await service.Users.Messages.Send(message, "me").ExecuteAsync();
            Console.WriteLine($"[Gmail] E-mail de confirmação enviado para {pedido.EmailCliente}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Gmail Error] Falha ao enviar e-mail: {ex.Message}");
            // Logar silenciosamente para não quebrar o fluxo de pedido
        }
    }

    private async Task<Google.Apis.Gmail.v1.GmailService> InitializeGmailServiceAsync()
    {
        UserCredential credential;

        using (var stream = new FileStream(CredentialsFilePath, FileMode.Open, FileAccess.Read))
        {
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(TokenDirectoryPath, true));
        }

        return new Google.Apis.Gmail.v1.GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    private Message CreateEmailMessage(Pedido pedido)
    {
        var message = new MimeMessage();
        message.To.Add(new MailboxAddress(pedido.NomeCliente, pedido.EmailCliente));
        // O "From" será automaticamente o e-mail autenticado no credentials.json
        message.From.Add(new MailboxAddress("NuvPizza Delivery", "nao-responda@nuvpizza.com")); 
        message.Subject = $"Oba! Seu pedido #{pedido.Id.ToString().Substring(0, 8).ToUpper()} foi recebido! 🍕";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = GerarTemplateHtml(pedido)
        };
        message.Body = bodyBuilder.ToMessageBody();

        // Converter MimeMessage em Raw (Base64UrlEncoded) que a API do Gmail exige
        using (var memoryStream = new MemoryStream())
        {
            message.WriteTo(memoryStream);
            var base64UrlEncoded = Convert.ToBase64String(memoryStream.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");

            return new Message { Raw = base64UrlEncoded };
        }
    }

    private string GerarTemplateHtml(Pedido pedido)
    {
        var itensHtml = string.Join("", pedido.Itens.Select(i => $@"
            <tr>
                <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{i.Quantidade}x {i.Nome}</td>
                <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>R$ {i.Total.ToString("F2")}</td>
            </tr>
        "));

        return $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>
            <div style='background-color: #ff4757; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                <h1>NuvPizza 🍕</h1>
                <p>Obrigado pelo seu pedido, {pedido.NomeCliente}!</p>
            </div>
            <div style='padding: 20px; border: 1px solid #eee; border-top: none; border-radius: 0 0 8px 8px;'>
                <p>Nossa cozinha já recebeu o seu pedido e estamos nos preparando para deixá-lo delicioso.</p>
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='margin-top: 0;'>Acompanhe em Tempo Real</h3>
                    <p>Você pode acompanhar o status da sua entrega através do nosso painel interativo:</p>
                    <a href='http://localhost:4200/acompanhar/{pedido.Id}' style='display: inline-block; background-color: #ff4757; color: white; text-decoration: none; padding: 10px 20px; border-radius: 5px; font-weight: bold;'>
                        Acompanhar Meu Pedido
                    </a>
                </div>
                
                <h3>Resumo do Pedido #{pedido.Id.ToString().Substring(0, 8).ToUpper()}</h3>
                <table style='width: 100%; border-collapse: collapse;'>
                    {itensHtml}
                    <tr>
                        <td style='padding: 10px; font-weight: bold;'>Taxa de Entrega</td>
                        <td style='padding: 10px; font-weight: bold; text-align: right;'>R$ {pedido.ValorFrete.ToString("F2")}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; font-size: 1.2em; font-weight: bold; color: #ff4757;'>Total</td>
                        <td style='padding: 10px; font-size: 1.2em; font-weight: bold; color: #ff4757; text-align: right;'>R$ {pedido.ValorTotal.ToString("F2")}</td>
                    </tr>
                </table>
                <br>
                <p><strong>Endereço de Entrega:</strong> {pedido.Logradouro}, {pedido.Numero} {pedido.Complemento} - {pedido.BairroNome}</p>
                <p><strong>Forma de Pagamento:</strong> {pedido.FormaPagamento}</p>
            </div>
        </div>";
    }
}
