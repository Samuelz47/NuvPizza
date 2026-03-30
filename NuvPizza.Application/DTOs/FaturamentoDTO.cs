namespace NuvPizza.Application.DTOs;

public class FaturamentoDTO
{
    public decimal Faturamento { get; set; }
    public int QuantidadePedidos { get; set; }
    public decimal Frete { get; set; }
    public decimal TicketMedio { get; set; }
    public List<FaturamentoPorMotoboyResumoDTO> FretesPorMotoboy { get; set; } = new();
}

public class FaturamentoPorMotoboyResumoDTO
{
    public string NomeMotoboy { get; set; } = string.Empty;
    public decimal TotalFrete { get; set; }
    public int QuantidadePedidos { get; set; }
}