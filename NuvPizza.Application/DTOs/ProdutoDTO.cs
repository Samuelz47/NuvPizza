using NuvPizza.Domain.Enums;

namespace NuvPizza.Application.DTOs;

public class ProdutoDTO
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public string ImagemUrl { get; set; }
    public int Categoria { get; set; }
    public int Tamanho { get; set; }
    public bool Ativo { get; set; }

    public ICollection<ComboItemTemplateDTO> ComboTemplates { get; set; } = new List<ComboItemTemplateDTO>();
}