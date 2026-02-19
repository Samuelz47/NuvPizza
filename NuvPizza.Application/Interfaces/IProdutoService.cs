using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Interfaces;

public interface IProdutoService
{
    Task<Result<ProdutoDTO>> CreateProdutoAsync(ProdutoForRegistrationDTO produtoForRegister);
    Task<Result<ProdutoDTO>> UpdateProdutoAsync(int id, ProdutoForUpdateDTO produtoForUpdate);
    Task<ProdutoDTO?> GetProdutoAsync(int id);
    Task<IEnumerable<ProdutoDTO>> GetAllProdutosAsync();
    Task<Result<ProdutoDTO>> DeleteProdutoAsync(int id);
}