namespace NuvPizza.Domain.Entities;

public class Bairro
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public decimal ValorFrete { get; set; }
    public bool Ativo { get; set; }
}