namespace NuvPizza.Application.DTOs;

public class CupomForRegistrationDTO
{
    public string Codigo { get; set; }
    public decimal? DescontoPorcentagem { get; set; }
    public bool FreteGratis { get; set; }
    public decimal PedidoMinimo { get; set; }
}