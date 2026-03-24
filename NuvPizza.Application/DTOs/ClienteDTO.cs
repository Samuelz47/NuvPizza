namespace NuvPizza.Application.DTOs;

public class ClienteDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int QuantidadePedidos { get; set; }
    public decimal ValorTotalGasto { get; set; }
    public DateTime DataPrimeiroPedido { get; set; }
    public DateTime DataUltimoPedido { get; set; }
}
