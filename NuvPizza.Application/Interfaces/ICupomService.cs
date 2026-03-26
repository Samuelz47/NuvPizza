using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Interfaces;

public interface ICupomService
{
    Task<IEnumerable<CupomDTO>> GetAllAsync();
    Task<Result<CupomDTO>> GetByCodeAsync(string codigo, string? telefone = null);
    Task<Result<CupomDTO>>  CreateAsync(CupomForRegistrationDTO cupomDto);
    Task<bool> DeleteAsync(int id);
}