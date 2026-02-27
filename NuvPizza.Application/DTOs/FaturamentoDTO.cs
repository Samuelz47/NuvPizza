namespace NuvPizza.Application.DTOs;

public class FaturamentoDTO
{
    public decimal Faturamento { get; set; }
    public int QuantidadePedidos { get; set; }
    public decimal Frete { get; set; }
    public decimal TicketMedio { get; set; }
}