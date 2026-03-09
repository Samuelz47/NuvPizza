using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Interfaces;
using NuvPizza.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.RateLimiting;

namespace NuvPizza.API.Controllers;
[Route("[controller]")]
[ApiController]

public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;
    private readonly ICacheService _cacheService;
    private const string ProdutosCachePrefix = "produtos_";

    public ProdutosController(IProdutoService produtoService, ICacheService cacheService)
    {
        _produtoService = produtoService;
        _cacheService = cacheService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post([FromForm] ProdutoForRegistrationDTO produtoDto)
    {
        var produtoCreated = await _produtoService.CreateProdutoAsync(produtoDto);

        if (!produtoCreated.IsSuccess)
        {
            return BadRequest(new { error = produtoCreated.Message });
        }

        // Limpa o cache para que o cardápio atualize imediatamente
        await _cacheService.RemoveByPrefixAsync(ProdutosCachePrefix);

        return Created($"/api/produtos/{produtoCreated.Data.Id}", produtoCreated.Data);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Put(int id, [FromForm] ProdutoForUpdateDTO produtoDto)
    {
        if (id != produtoDto.Id) return BadRequest("IDs não conferem");

        var result = await _produtoService.UpdateProdutoAsync(id, produtoDto);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Message });
        }

        // Limpa o cache após a edição
        await _cacheService.RemoveByPrefixAsync(ProdutosCachePrefix);

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [EnableRateLimiting("PublicApiLimit")]
    public async Task<IActionResult> GetProdutos(int id)
    {
        string cacheKey = $"{ProdutosCachePrefix}id_{id}";
        var produtoDto = await _cacheService.GetAsync<ProdutoDTO>(cacheKey);

        if (produtoDto is null)
        {
            produtoDto = await _produtoService.GetProdutoAsync(id);
            
            if (produtoDto is null) { return NotFound("Produto não encontrado"); }

            // Salva no cache por 1 dia
            await _cacheService.SetAsync(cacheKey, produtoDto, TimeSpan.FromDays(1));
        }
        
        return Ok(produtoDto);
    }

    [HttpGet]
    [EnableRateLimiting("PublicApiLimit")]
    public async Task<ActionResult> GetAllProdutos()
    {
        string cacheKey = $"{ProdutosCachePrefix}all";
        var produtosDto = await _cacheService.GetAsync<IEnumerable<ProdutoDTO>>(cacheKey);

        if (produtosDto is null)
        {
            produtosDto = await _produtoService.GetAllProdutosAsync();
            
            // Salva no cache por 1 dia
            await _cacheService.SetAsync(cacheKey, produtosDto, TimeSpan.FromDays(1));
        }

        return Ok(produtosDto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _produtoService.DeleteProdutoAsync(id);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Message });
        }

        // Limpa o cache após a exclusão
        await _cacheService.RemoveByPrefixAsync(ProdutosCachePrefix);

        return NoContent();
    }
}