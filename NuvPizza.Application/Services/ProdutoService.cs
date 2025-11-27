using AutoMapper;
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

    public async Task<ProdutoDTO> CreateProdutoAsync(ProdutoForRegistrationDTO produtoForRegister)
    {
        if (produtoForRegister is null)
        {
            throw new InvalidOperationException("Produto não pode ser nulo");
        }

        if (produtoForRegister.Preco < 0)
        {
            throw new InvalidOperationException("Preço inválido");
        }

        if (produtoForRegister.CategoriaId > 5 || produtoForRegister.CategoriaId < 0)
        {
            throw new InvalidOperationException("Categoria inválida");
        }

        var produto = _mapper.Map<Produto>(produtoForRegister);
        _produtoRepository.Create(produto);
        await _uow.CommitAsync();
        
        var produtoDto = _mapper.Map<ProdutoDTO>(produto);
        return produtoDto;
    }
}