using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Interfaces;

public interface IProdutoService
{
    Task<ProdutoDTO> CreateProdutoAsync(ProdutoForRegistrationDTO produtoForRegister);
}