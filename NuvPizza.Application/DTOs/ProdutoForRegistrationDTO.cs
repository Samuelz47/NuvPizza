using Microsoft.AspNetCore.Http;
using NuvPizza.Domain.Enums;

namespace NuvPizza.Application.DTOs;

public class ProdutoForRegistrationDTO
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public IFormFile? Imagem { get; set; }
    public Categoria Categoria { get; set; }
    public Tamanho Tamanho { get; set; }

    // Use string if sending via FormData to deserialize later, but for now we define as string
    public string? ComboTemplatesJson { get; set; }
}