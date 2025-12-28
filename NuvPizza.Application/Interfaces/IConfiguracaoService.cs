using NuvPizza.Application.DTOs;

namespace NuvPizza.Application.Interfaces;

public interface IConfiguracaoService
{
    Task<bool> AberturaDeLojaAsync(AbrirLojaDTO abrirLojaDto);
    Task<bool> FecharLojaAsync();
    Task<bool> EstenderLojaAsync(EstenderLojaDTO estenderLojaDto);
}