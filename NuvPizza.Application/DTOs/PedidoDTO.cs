using NuvPizza.Domain.Enums;

namespace NuvPizza.Application.DTOs;

public class PedidoDTO
{
    public Guid Id { get; set; }
    public string NomeCliente { get; set; }
    public string LinkWhatsapp { get; set; }
    public string Cep { get; set; }
    public string Logradouro { get; set; }
    public string Bairro { get; set; }
    public string Numero { get; set; }
    public string? Complemento { get; set; }
    public StatusPedido StatusPedido { get; set; }
    public DateTime DataPedido { get; set; }
    public string FormaPagamento { get; set; }
    public List<ItemPedidoDTO> Itens { get; set; }
}