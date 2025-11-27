namespace NuvPizza.Application.DTOs;

public class ProdutoForRegistrationDTO
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public string ImagemUrl { get; set; }
    public int CategoriaId { get; set; }
    public string Tamanho { get; set; }
}