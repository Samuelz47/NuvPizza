using NuvPizza.Domain.Enums;

namespace NuvPizza.Application.DTOs;

public class PedidoDTO
{
    public Guid Id { get; set; }
    public string NomeCliente { get; set; }
    public string LinkWhatsapp { get; set; }
    public string Cep { get; set; }
    public string Logradouro { get; set; }
    public string BairroNome { get; set; }
    public int BairroId { get; set; }
    public string Numero { get; set; }
    public string? Complemento { get; set; }
    public StatusPedido StatusPedido { get; set; }
    public DateTime DataPedido { get; set; }
    public string FormaPagamento { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorTotal { get; set; }
    public List<ItemPedidoDTO> Itens { get; set; }
}