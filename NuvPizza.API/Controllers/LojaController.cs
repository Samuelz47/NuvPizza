using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;

namespace NuvPizza.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class LojaController : ControllerBase
{
    private readonly IConfiguracaoService _configuracaoService;

    public LojaController(IConfiguracaoService configuracaoService)
    {
        _configuracaoService = configuracaoService;
    }

    [HttpPost("estender")]
    public async Task<IActionResult> Estender([FromBody] EstenderLojaDTO estenderLojaDto)
    {
        var result = await _configuracaoService.EstenderLojaAsync(estenderLojaDto);

        if (!result) return BadRequest("Loja está fechada");
        return Ok();
    }

    [HttpPost("fechar")]
    public async Task<IActionResult> Fechamento()
    {
        await  _configuracaoService.FecharLojaAsync();
        return Ok();
    }

    [HttpPost("abrir")]
    public async Task<IActionResult> Abertura([FromBody] AbrirLojaDTO abrirLojaDto)
    {
        await _configuracaoService.AberturaDeLojaAsync(abrirLojaDto);
        return Ok();
    }
}