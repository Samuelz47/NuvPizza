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

    public CupomService(ICupomRepository cupomRepository, IUnitOfWork uow, IMapper mapper)
    {
        _cupomRepository = cupomRepository;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CupomDTO>> GetAllAsync()
    {
        var cupons = await _cupomRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CupomDTO>>(cupons);
    }

    public async Task<Result<CupomDTO>> GetByCodeAsync(string codigo)
    {
        var cupom = await _cupomRepository.GetAsync(c => c.Codigo == codigo);
        
        if (cupom is null) return Result<CupomDTO>.Failure("Cupom inexistente");
        if (cupom.Ativo == false) return Result<CupomDTO>.Failure("Cupom Inativo");
        
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
        cupom.Ativo = true;
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