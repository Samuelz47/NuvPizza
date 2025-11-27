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
}