using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Interfaces;

public interface IProdutoService
{
    Task<Result<ProdutoDTO>> CreateProdutoAsync(ProdutoForRegistrationDTO produtoForRegister);
}