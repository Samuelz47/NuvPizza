using AutoMapper;
using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Application.Services;

public class CupomService : ICupomService
{
    private readonly ICupomRepository _cupomRepository;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPedidoRepository _pedidoRepository;

    public CupomService(ICupomRepository cupomRepository, IUnitOfWork uow, IMapper mapper, IPedidoRepository pedidoRepository)
    {
        _cupomRepository = cupomRepository;
        _uow = uow;
        _mapper = mapper;
        _pedidoRepository = pedidoRepository;
    }

    public async Task<IEnumerable<CupomDTO>> GetAllAsync()
    {
        var cupons = await _cupomRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CupomDTO>>(cupons);
    }

    public async Task<Result<CupomDTO>> GetByCodeAsync(string codigo, string? telefone = null)
    {
        var cupom = await _cupomRepository.GetAsync(c => c.Codigo == codigo);
        
        if (cupom is null) return Result<CupomDTO>.Failure("Cupom inexistente");
        if (cupom.Ativo == false) return Result<CupomDTO>.Failure("Cupom Inativo");

        if (!string.IsNullOrWhiteSpace(telefone))
        {
            var jaUtilizou = await _pedidoRepository.VerificarCupomUtilizadoAsync(telefone, cupom.Id);
            if (jaUtilizou)
            {
                return Result<CupomDTO>.Failure("Você já utilizou este cupom anteriormente.");
            }
        }
        
        var dto = _mapper.Map<CupomDTO>(cupom);
        return Result<CupomDTO>.Success(dto);
    }

    public async Task<Result<CupomDTO>> CreateAsync(CupomForRegistrationDTO cupomDto)
    {
        if (cupomDto is null) return Result<CupomDTO>.Failure("Cupom nulo");
        if (cupomDto.Codigo.Any(char.IsWhiteSpace))
        {
            return Result<CupomDTO>.Failure("O código do cupom não pode conter espaços.");
        }
        if ((!cupomDto.DescontoPorcentagem.HasValue || cupomDto.DescontoPorcentagem == 0) && !cupomDto.FreteGratis)
        {
            return Result<CupomDTO>.Failure("O cupom deve oferecer um desconto em porcentagem (maior que 0) ou frete grátis.");
        }
        if (cupomDto.DescontoPorcentagem.HasValue)
        {
            if (cupomDto.DescontoPorcentagem < 0 || cupomDto.DescontoPorcentagem > 100)
            {
                return Result<CupomDTO>.Failure("A porcentagem de desconto deve ser entre 0 e 100.");
            }
        }
        
        var cupom = _mapper.Map<Cupom>(cupomDto);
        cupom.PedidoMinimo = cupomDto.PedidoMinimo; // Garantindo atribuição
        cupom.Ativo = true;
        
        Console.WriteLine($"[DEBUG] Criando Cupom: {cupom.Codigo}, Valor Minimo: {cupom.PedidoMinimo}");
        
        _cupomRepository.Create(cupom);
        await _uow.CommitAsync();
        var dto = _mapper.Map<CupomDTO>(cupom);
        return Result<CupomDTO>.Success(dto);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cupom = await _cupomRepository.GetAsync(c => c.Id == id);
        if (cupom is null) return false;
        
        _cupomRepository.Delete(cupom);
        await _uow.CommitAsync();
        return true;
    }
}