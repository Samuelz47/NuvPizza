namespace NuvPizza.Application.DTOs;

public class CriarPreferenceDTO
{
    public string Titulo { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public string? EmailPagador { get; set; }
    public string? ExternalReference { get; set; }
}