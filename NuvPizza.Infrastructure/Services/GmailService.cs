using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using NuvPizza.Domain.Interfaces;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Infrastructure.Services;

public class GmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public GmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task EnviarEmailConfirmacao(Pedido pedido)
    {
        if (string.IsNullOrWhiteSpace(pedido.EmailCliente))
        {
            return;
        }

        try
        {
            var email = new MimeMessage();
            var fromEmail = _configuration["EmailSettings:Email"];
            var appPassword = _configuration["EmailSettings:Password"];

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(appPassword))
            {
                Console.WriteLine("[Gmail Error] Configurações de e-mail (EmailSettings:Email ou EmailSettings:Password) não encontradas.");
                return;
            }

            email.From.Add(new MailboxAddress("NuvPizza Delivery", fromEmail));
            email.To.Add(new MailboxAddress(pedido.NomeCliente, pedido.EmailCliente));
            email.Subject = $"Oba! Seu pedido #{pedido.Id.ToString().Substring(0, 8).ToUpper()} foi recebido! 🍕";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = GerarTemplateHtml(pedido)
            };
            email.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Conecta ao SMTP do Gmail
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                // Autentica com o e-mail e a senha de aplicativo
                await client.AuthenticateAsync(adminEmail, appPassword);

                await client.SendAsync(email);
                await client.DisconnectAsync(true);
            }

            Console.WriteLine($"[Gmail] E-mail de confirmação enviado via SMTP para {pedido.EmailCliente}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Gmail Error] Falha ao enviar e-mail via SMTP: {ex.Message}");
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
            <div style='background-color: #4B6FA5; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                <h1>NuvPizza 🍕</h1>
                <p>Obrigado pelo seu pedido, {pedido.NomeCliente}!</p>
            </div>
            <div style='padding: 20px; border: 1px solid #eee; border-top: none; border-radius: 0 0 8px 8px;'>
                <p>Nossa cozinha já recebeu o seu pedido e estamos nos preparando para deixá-lo delicioso.</p>
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='margin-top: 0;'>Acompanhe em Tempo Real</h3>
                    <p>Você pode acompanhar o status da sua entrega através do nosso painel interativo:</p>
                    <a href='https://nuvpizza.com.br/acompanhar/{pedido.Id}' style='display: inline-block; background-color: #4B6FA5; color: white; text-decoration: none; padding: 10px 20px; border-radius: 5px; font-weight: bold;'>
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
                        <td style='padding: 10px; font-size: 1.2em; font-weight: bold; color: #4B6FA5;'>Total</td>
                        <td style='padding: 10px; font-size: 1.2em; font-weight: bold; color: #4B6FA5; text-align: right;'>R$ {pedido.ValorTotal.ToString("F2")}</td>
                    </tr>
                </table>
                <br>
                <p><strong>Endereço de Entrega:</strong> {pedido.Logradouro}, {pedido.Numero} {pedido.Complemento} - {pedido.BairroNome}</p>
                <p><strong>Forma de Pagamento:</strong> {pedido.FormaPagamento}</p>
            </div>
        </div>";
    }
}
