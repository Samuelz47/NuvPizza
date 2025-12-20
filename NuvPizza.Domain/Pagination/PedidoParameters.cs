using NuvPizza.Domain.Enums;

namespace NuvPizza.Domain.Pagination;

public class PedidoParameters : QueryParameters
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public List<StatusPedido>? Status { get; set; }
    public string? NomeCliente { get; set; }
    public decimal? ValorMinimo { get; set; }
    public decimal? ValorMaximo { get; set; }
}