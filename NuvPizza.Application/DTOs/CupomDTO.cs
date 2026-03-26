namespace NuvPizza.Application.DTOs;

public class CupomDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; }
    public decimal? DescontoPorcentagem { get; set; }
    public bool Ativo { get; set; }
    public bool FreteGratis { get; set; }
    public decimal PedidoMinimo { get; set; }
}