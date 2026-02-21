using Microsoft.AspNetCore.Http;
using NuvPizza.Domain.Enums;

namespace NuvPizza.Application.DTOs;

public class ProdutoForUpdateDTO
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public IFormFile? Imagem { get; set; }
    public Categoria Categoria { get; set; }
    public Tamanho Tamanho { get; set; }
    public bool Ativo { get; set; }

    // Use string if sending via FormData to deserialize later
    public string? ComboTemplatesJson { get; set; }
}
