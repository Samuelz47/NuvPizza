using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Interfaces;

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

    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _configuracaoService.GetStatusLojaAsync();
        return Ok(status);
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

    [HttpPut("video-destaque")]
    public async Task<IActionResult> AtualizarVideoDestaque([FromBody] AtualizarVideoDestaqueDTO dto)
    {
        await _configuracaoService.AtualizarVideoDestaqueAsync(dto);
        return Ok();
    }
}