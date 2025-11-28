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
        
        var produto = _mapper.Map<Produto>(produtoForRegister);
        _produtoRepository.Create(produto);
        await _uow.CommitAsync();
        
        var produtoDto = _mapper.Map<ProdutoDTO>(produto);
        return Result<ProdutoDTO>.Success(produtoDto);
    }

    public async Task<ProdutoDTO?> GetProdutoAsync(int id)
    {
        var produto = await _produtoRepository.GetAsync(p => p.Id == id);
        if (produto is null) { return null; }
        
        var produtoDto = _mapper.Map<ProdutoDTO>(produto);
        return produtoDto;    
    }

    public async Task<IEnumerable<ProdutoDTO>> GetAllProdutosAsync()
    {
        var produtos = await _produtoRepository.GetAllAsync();
        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
        return produtosDto;
    }
}