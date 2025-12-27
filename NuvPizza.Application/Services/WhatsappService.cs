using System.Text;
using Microsoft.Extensions.Configuration;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Services;

public class WhatsappService : IWhatsappService
{
    private readonly IConfiguration _configuration;

    public WhatsappService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GerarLinkPedido(Pedido pedido)
    {
        var telefone = _configuration["ConfiguracoesPizzaria:TelefoneWhatsapp"];

        if (string.IsNullOrEmpty(telefone)) return "";

        var sb = new StringBuilder();
        
        sb.AppendLine($" *Novo Pedido*");
        sb.AppendLine($"--------------------------------");
        sb.AppendLine($" *Cliente:* {pedido.NomeCliente}");
        sb.AppendLine($" *Tel:* {pedido.TelefoneCliente}");

        sb.AppendLine($" *Entrega:* {pedido.Logradouro}, {pedido.Numero}");
        if (!string.IsNullOrEmpty(pedido.Complemento))
        {
            sb.AppendLine($"   ({pedido.Complemento})");
        }
        sb.AppendLine($"   {pedido.BairroNome}");
        sb.AppendLine($"--------------------------------");
        sb.AppendLine($"*RESUMO DO PEDIDO*");

        foreach (var item in pedido.Itens)
        {
            sb.AppendLine($"{item.Quantidade}x {item.Nome} ({item.Total:C})");
        }

        sb.AppendLine($"--------------------------------");
        sb.AppendLine($"*TOTAL:* {pedido.ValorTotal:C}");
        sb.AppendLine($"*Pagamento:* {pedido.FormaPagamento}");

        //Encode da URL (Transforma espaço em %20, quebra de linha em %0A)
        var mensagemCodificada = Uri.EscapeDataString(sb.ToString());
        return $"https://wa.me/{telefone}?text={mensagemCodificada}";
    }
}