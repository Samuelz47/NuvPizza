namespace NuvPizza.Application.DTOs;

public class PedidoForRegistrationDTO
{
    public string NomeCliente { get; set; }
    public string TelefoneCliente { get; set; }
    public string EnderecoEntrega { get; set; }
    public string FormaPagamento { get; set; }

    public List<ItemPedidoForRegistrationDTO> Itens { get; set; }
}