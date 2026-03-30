using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;

namespace NuvPizza.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MotoboyController : ControllerBase
    {
        private readonly IMotoboyService _motoboyService;

        public MotoboyController(IMotoboyService motoboyService)
        {
            _motoboyService = motoboyService;
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            var result = await _motoboyService.ObterTodosAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Message);
        }

        [HttpGet("ativos")]
        public async Task<IActionResult> ObterAtivos()
        {
            var result = await _motoboyService.ObterAtivosAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Message);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var result = await _motoboyService.ObterPorIdAsync(id);
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.Message);
        }

        [HttpPost]
        public async Task<IActionResult> Criar(MotoboyCreateDTO dto)
        {
            var result = await _motoboyService.CriarAsync(dto);
            return result.IsSuccess ? CreatedAtAction(nameof(ObterPorId), new { id = result.Data.Id }, result.Data) : BadRequest(result.Message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(Guid id, MotoboyUpdateDTO dto)
        {
            if (id != dto.Id) return BadRequest("ID do motoboy não confere");
            var result = await _motoboyService.AtualizarAsync(dto);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(Guid id)
        {
            var result = await _motoboyService.DeletarAsync(id);
            return result.IsSuccess ? Ok(new { success = true }) : BadRequest(result.Message);
        }

        [HttpGet("{id}/faturamento")]
        public async Task<IActionResult> ObterFaturamento(Guid id, [FromQuery] DateTime dataInicial, [FromQuery] DateTime dataFinal)
        {
            var result = await _motoboyService.ObterFaturamentoIndividualAsync(id, dataInicial, dataFinal);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Message);
        }
    }
}
