using AutoMapper;
using Microsoft.AspNetCore.Http;
using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Interfaces;
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

        if (produtoForRegister.Imagem != null)
        {
            var caminhoImagem = await SalvarArquivo(produtoForRegister.Imagem);
            produto.ImagemUrl = caminhoImagem;
        }
        else
        {
            produto.ImagemUrl = "images/sem_imagem.png";
        }

        if (produto.Categoria == Domain.Enums.Categoria.Combo && !string.IsNullOrEmpty(produtoForRegister.ComboTemplatesJson))
        {
            var templatesDto = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ComboItemTemplateDTO>>(produtoForRegister.ComboTemplatesJson);
            if (templatesDto != null && templatesDto.Any())
            {
                produto.ComboTemplates = _mapper.Map<List<ComboItemTemplate>>(templatesDto);
            }
        }
        
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

    public async Task<Result<ProdutoDTO>> DeleteProdutoAsync(int id)
    {
        var produto = await _produtoRepository.GetAsync(p => p.Id == id);
        if (produto is null) return Result<ProdutoDTO>.Failure("O produto não existe");
        
        _produtoRepository.Delete(produto);
        await _uow.CommitAsync();
        
        var produtoDto = _mapper.Map<ProdutoDTO>(produto);
        return Result<ProdutoDTO>.Success(produtoDto);
    }

    public async Task<Result<ProdutoDTO>> UpdateProdutoAsync(int id, ProdutoForUpdateDTO produtoDto)
    {
        if (produtoDto is null) return Result<ProdutoDTO>.Failure("Dados inválidos");
        
        var produto = await _produtoRepository.GetAsync(p => p.Id == id);
        if (produto is null) return Result<ProdutoDTO>.Failure("Produto não encontrado");

        // Regra de Negócio: Tamanho
        if (produtoDto.Categoria == Domain.Enums.Categoria.Pizza)
        {
            produto.Tamanho = produtoDto.Tamanho;
        }
        else
        {
            produto.Tamanho = Domain.Enums.Tamanho.Unico;
        }

        // Atualiza campos básicos
        produto.Nome = produtoDto.Nome;
        produto.Descricao = produtoDto.Descricao;
        produto.Preco = produtoDto.Preco;
        produto.Categoria = produtoDto.Categoria;
        produto.Ativo = produtoDto.Ativo;

        // Atualiza Imagem se fornecida
        if (produtoDto.Imagem != null)
        {
            var caminhoImagem = await SalvarArquivo(produtoDto.Imagem);
            produto.ImagemUrl = caminhoImagem;
        }

        // Atualiza os templates de combo
        if (produto.Categoria == Domain.Enums.Categoria.Combo)
        {
            // Limpa os antigos (por causa do Cascade Delete configurado no EF)
            if (produto.ComboTemplates != null)
            {
                produto.ComboTemplates.Clear();
            }
            else
            {
                produto.ComboTemplates = new List<ComboItemTemplate>();
            }

            if (!string.IsNullOrEmpty(produtoDto.ComboTemplatesJson))
            {
                var templatesDto = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ComboItemTemplateDTO>>(produtoDto.ComboTemplatesJson);
                if (templatesDto != null && templatesDto.Any())
                {
                    foreach (var templateDto in templatesDto)
                    {
                        produto.ComboTemplates.Add(_mapper.Map<ComboItemTemplate>(templateDto));
                    }
                }
            }
        }
        else
        {
            // Se não for combo, garante que não tenha templates
            produto.ComboTemplates?.Clear();
        }

        _produtoRepository.Update(produto);
        await _uow.CommitAsync();

        var produtoResult = _mapper.Map<ProdutoDTO>(produto);
        return Result<ProdutoDTO>.Success(produtoResult);
    }
    
    private async Task<string> SalvarArquivo(IFormFile arquivo)
    {
        var nomeArquivo = $"{Guid.NewGuid()}{Path.GetExtension(arquivo.FileName)}";
        var caminhoPasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
    
        if (!Directory.Exists(caminhoPasta))  Directory.CreateDirectory(caminhoPasta);

        var caminhoCompleto = Path.Combine(caminhoPasta, nomeArquivo);

        using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
        {
            await arquivo.CopyToAsync(stream);
        }

        return $"/images/{nomeArquivo}";
    }
}