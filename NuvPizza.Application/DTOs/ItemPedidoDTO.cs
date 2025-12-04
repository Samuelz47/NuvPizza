namespace NuvPizza.Application.DTOs;

public class ItemPedidoDTO
{
    public string NomeProduto { get; set; } = string.Empty; // Para mostrar o nome
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; }
    public decimal Total { get; set; }
}