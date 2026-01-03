using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;

namespace NuvPizza.API.Controllers;

[ApiController]
[Route("[controller]")]
public class PagamentoController : ControllerBase
{
    private readonly IPagamentoService _pagamentoService;

    public PagamentoController(IPagamentoService pagamentoService)
    {
        _pagamentoService = pagamentoService;
    }

    [HttpPost]
    public async Task<IActionResult> ProcessarPagamento([FromBody] PagamentoRequestDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _pagamentoService.ProcessarPagamentoAsync(dto);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.Message);
    }
}