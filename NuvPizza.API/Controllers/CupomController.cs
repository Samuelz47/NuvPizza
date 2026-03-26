using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;

namespace NuvPizza.API.Controllers;

[ApiController]
[Route("[controller]")]

public class CupomController : ControllerBase
{
    private readonly ICupomService  _cupomService;

    public CupomController(ICupomService cupomService)
    {
        _cupomService = cupomService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostAsync([FromBody] CupomForRegistrationDTO cupomDto)
    {
        var cupomCreated = await _cupomService.CreateAsync(cupomDto);
        
        if (!cupomCreated.IsSuccess)
        {
            return BadRequest(new { error = cupomCreated.Message });
        }
        
        return Created($"/api/produtos/{cupomCreated.Data.Id}", cupomCreated.Data);
    }

    [HttpGet("{codigo}")]
    public async Task<IActionResult> GetAsync([FromRoute] string codigo, [FromQuery] string? telefone)
    {
        var cupom = await _cupomService.GetByCodeAsync(codigo, telefone);
        
        if (!cupom.IsSuccess) return BadRequest(new { error = cupom.Message });
        
        return Ok(cupom.Data);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult> GetAllAsync()
    {
        var cupons = await _cupomService.GetAllAsync();
        
        return Ok(cupons);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteAsync([FromRoute] int id)
    {
        bool cupomExcluded = await _cupomService.DeleteAsync(id);
        
        if (cupomExcluded) return Ok(cupomExcluded);
        
        return BadRequest("Cupom não encontrado");
    }
}