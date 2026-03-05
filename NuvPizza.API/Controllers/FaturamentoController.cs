using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.Interfaces;

namespace NuvPizza.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FaturamentoController : ControllerBase
{
    private readonly IFaturamentoService _faturamentoService;

    public FaturamentoController(IFaturamentoService faturamentoService)
    {
        _faturamentoService = faturamentoService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterFaturamento([FromQuery] DateTime inicial, [FromQuery] DateTime final)
    {
        if (inicial == default) 
            inicial = new DateTime(DateTime.UtcNow.AddHours(-3).Year, DateTime.UtcNow.AddHours(-3).Month, 1);
            
        if (final == default) 
            final = new DateTime(DateTime.UtcNow.AddHours(-3).Year, DateTime.UtcNow.AddHours(-3).Month, DateTime.DaysInMonth(DateTime.UtcNow.AddHours(-3).Year, DateTime.UtcNow.AddHours(-3).Month));
        
        var resultado = await _faturamentoService.ObterFaturamentoAsync(inicial, final);
        if (!resultado.IsSuccess) return BadRequest(new { message = resultado.Message });
        
        return Ok(resultado.Data);
    }
    
}