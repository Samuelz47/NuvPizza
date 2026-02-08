using Microsoft.AspNetCore.Http; 

namespace NuvPizza.Application.DTOs;

public class PedidoForRegistrationDTO
{
    public string NomeCliente { get; set; }
    public string TelefoneCliente { get; set; }
    public string EmailCliente { get; set; }
    public string Cep { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string? Complemento { get; set; }
    public string FormaPagamento { get; set; }
    public List<ItemPedidoForRegistrationDTO> Itens { get; set; }
    public string? Observacao { get; set; }
}