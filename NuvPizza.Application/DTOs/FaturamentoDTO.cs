namespace NuvPizza.Application.DTOs;

public class FaturamentoDTO
{
    public decimal Faturamento { get; set; }
    public int QunatidadePedido { get; set; }
    public decimal Frete { get; set; }
    public decimal TicktedMedio { get; set; }
}