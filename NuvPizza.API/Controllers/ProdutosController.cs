using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public async Task<IActionResult> Post([FromForm] ProdutoForRegistrationDTO produtoDto)
    {
        var produtoCreated = await _produtoService.CreateProdutoAsync(produtoDto);

        if (!produtoCreated.IsSuccess)
        {
            return BadRequest(new { error = produtoCreated.Message });
        }
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
        return Ok(result.Data);
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

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _produtoService.DeleteProdutoAsync(id);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Message });
        }
        return NoContent();
    }
}