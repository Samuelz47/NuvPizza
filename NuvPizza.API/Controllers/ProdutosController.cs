using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Entities;

namespace NuvPizza.API.Controllers;
[Route("[controller]")]
[ApiController]

public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ProdutoForRegistrationDTO produtoDto)
    {
        var produtoCreated = await _produtoService.CreateProdutoAsync(produtoDto);

        if (!produtoCreated.IsSuccess)
        {
            return BadRequest(new { error = produtoCreated.Message });
        }
        return Created($"/api/produtos/{produtoCreated.Data.Id}", produtoCreated.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProdutos(int id)
    {
        var produtoDto = await _produtoService.GetProdutoAsync(id);
        
        if (produtoDto is null) { return NotFound("Produto não encontrado"); }
        
        return Ok(produtoDto);
    }

    [HttpGet]
    public async Task<ActionResult> GetAllProdutos()
    {
        var produtosDto = await _produtoService.GetAllProdutosAsync();
        return Ok(produtosDto);
    }
}