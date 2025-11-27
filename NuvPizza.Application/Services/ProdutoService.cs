using AutoMapper;
using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;

namespace NuvPizza.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ProdutoService(IProdutoRepository produtoRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _produtoRepository = produtoRepository;
        _uow = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProdutoDTO>> CreateProdutoAsync(ProdutoForRegistrationDTO produtoForRegister)
    {
        if (produtoForRegister is null)
        {
            return Result<ProdutoDTO>.Failure("Produto não pode ser nulo");
        }

        if (produtoForRegister.Preco < 0)
        {
            return Result<ProdutoDTO>.Failure("Preço inválido");
        }

        if (produtoForRegister.CategoriaId > 5 || produtoForRegister.CategoriaId < 0)
        {
            return Result<ProdutoDTO>.Failure("Categoria inválida");
        }

        var produto = _mapper.Map<Produto>(produtoForRegister);
        _produtoRepository.Create(produto);
        await _uow.CommitAsync();
        
        var produtoDto = _mapper.Map<ProdutoDTO>(produto);
        return Result<ProdutoDTO>.Success(produtoDto);
    }
}