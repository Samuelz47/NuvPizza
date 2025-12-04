namespace NuvPizza.Application.DTOs;

public class PedidoDTO
{
    public Guid Id { get; set; }
    public string NomeCliente { get; set; }
    public string TelefoneCliente { get; set; }
    public string EnderecoEntrega { get; set; }
    public string FormaPagamento { get; set; }
    public List<ItemPedidoDTO> Itens { get; set; }
}