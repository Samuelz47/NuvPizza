using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NuvPizza.Domain.Interfaces;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;

namespace NuvPizza.API.Controllers;

[ApiController]
[Route("[controller]")]

public class BairrosController : ControllerBase
{
    private readonly IBairroRepository _bairroRepository;
    private readonly ICacheService _cacheService;
    private const string BairrosCacheKey = "bairros_all";

    public BairrosController(IBairroRepository bairroRepository, ICacheService cacheService)
    {
        _bairroRepository = bairroRepository;
        _cacheService = cacheService;
    }

    [HttpGet]
    [EnableRateLimiting("PublicApiLimit")]
    public async Task<ActionResult> GetAll()
    {
        // 1. Tenta buscar do cache
        var bairros = await _cacheService.GetAsync<IEnumerable<Bairro>>(BairrosCacheKey);

        if (bairros is null)
        {
            // 2. Se não tem no cache, busca do banco
            bairros = await _bairroRepository.GetAllAsync();

            // 3. Salva no cache por 1 dia (bairros raramente mudam)
            await _cacheService.SetAsync(BairrosCacheKey, bairros, TimeSpan.FromDays(1));
        }

        return Ok(bairros);
    }
}