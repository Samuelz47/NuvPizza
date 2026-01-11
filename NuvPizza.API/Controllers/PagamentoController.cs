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

    [HttpPost("criar-link")]
    public async Task<IActionResult> CriarLinkPagamento([FromBody] CriarPreferenceDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _pagamentoService.CriarPreferenciaAsync(dto);

        if (result.IsSuccess)
        {
            return Ok(new { url = result.Data });
        }

        return BadRequest(result.Message);
    }
}