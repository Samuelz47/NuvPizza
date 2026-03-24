namespace NuvPizza.Domain.Entities;

public class Cliente
{
    public Cliente()
    {
        Id = Guid.NewGuid();
        Pedidos = new List<Pedido>();
    }

    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int QuantidadePedidos { get; set; }
    public decimal ValorTotalGasto { get; set; }
    public DateTime DataPrimeiroPedido { get; set; }
    public DateTime DataUltimoPedido { get; set; }
    public ICollection<Pedido> Pedidos { get; set; }
}
