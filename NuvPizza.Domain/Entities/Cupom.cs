namespace NuvPizza.Domain.Entities;

public class Cupom
{
    public int Id { get; set; }
    public string Codigo { get; set; }
    public decimal DescontoPorcentagem { get; set; }
    public bool Ativo { get; set; }
    public bool FreteGratis { get; set; }
}