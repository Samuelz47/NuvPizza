namespace NuvPizza.Domain.Entities;

public sealed class Configuracao
{
    public int Id { get; set; }
    public bool EstaAberta { get; set; } = false;
    public DateTime? DataHoraFechamentoAtual { get; set; }
    public TimeSpan HorarioFechamentoPadrao { get; set; }
    public string? VideoDestaqueUrl { get; set; }
}