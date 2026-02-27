namespace NuvPizza.Application.DTOs;

public class StatusLojaDTO
{
    public bool EstaAberta { get; set; }
    public DateTime? DataHoraFechamento { get; set; }
    public string? VideoDestaqueUrl { get; set; }
}
