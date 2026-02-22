using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Repositories;

namespace NuvPizza.Application.Services;

public class ConfiguracaoService : IConfiguracaoService
{
    private readonly IConfiguracaoRepository _configuracaoRepository;
    private readonly IUnitOfWork _uow;
    private readonly INotificacaoService _notificacaoService;

    public ConfiguracaoService(INotificacaoService notificacaoService, IUnitOfWork uow, IConfiguracaoRepository configuracaoRepository)
    {
        _notificacaoService = notificacaoService;
        _uow = uow;
        _configuracaoRepository = configuracaoRepository;
    }

    public async Task<bool> AberturaDeLojaAsync(AbrirLojaDTO abrirLojaDto)
    {
        var config = await _configuracaoRepository.GetAsync(c => c.Id == 1);

        var agora = DateTime.Now;
        var dataDeFechamento = agora.Date + abrirLojaDto.HoraDeEncerramento;
        
        if(dataDeFechamento <= agora) { dataDeFechamento = dataDeFechamento.AddDays(1); }
        
        
        config.EstaAberta = true;
        config.DataHoraFechamentoAtual = dataDeFechamento;
        
        _configuracaoRepository.Update(config);
        await _uow.CommitAsync();

        await _notificacaoService.NotificarAlteracaoStatusLoja(true, "A Pizzaria está aberta!");
        return true;
    }

    public async Task<bool> FecharLojaAsync()
    {
        var config = await _configuracaoRepository.GetAsync(c => c.Id == 1);
        config.EstaAberta =  false;
        config.DataHoraFechamentoAtual = null;
        _configuracaoRepository.Update(config);
        await _uow.CommitAsync();
        await _notificacaoService.NotificarAlteracaoStatusLoja(false, "Pizza fechada!");
        return true;
    }

    public async Task<bool> EstenderLojaAsync(EstenderLojaDTO estenderLojaDto)
    {
        var config = await _configuracaoRepository.GetAsync(c => c.Id == 1);
        
        if(!config.EstaAberta || config.DataHoraFechamentoAtual == null) return false;

        config.DataHoraFechamentoAtual = config.DataHoraFechamentoAtual.Value.AddMinutes(estenderLojaDto.MinutosExtras);

        _configuracaoRepository.Update(config);
        await _uow.CommitAsync();
        await _notificacaoService.NotificarAlteracaoStatusLoja(true, "Horário estendido!");
        return true;
    }

    public async Task<StatusLojaDTO> GetStatusLojaAsync()
    {
        var config = await _configuracaoRepository.GetAsync(c => c.Id == 1);
        
        // Auto-fecha se passou do horário
        if (config.EstaAberta && config.DataHoraFechamentoAtual.HasValue && DateTime.Now >= config.DataHoraFechamentoAtual.Value)
        {
            config.EstaAberta = false;
            config.DataHoraFechamentoAtual = null;
            _configuracaoRepository.Update(config);
            await _uow.CommitAsync();
        }

        return new StatusLojaDTO
        {
            EstaAberta = config.EstaAberta,
            DataHoraFechamento = config.DataHoraFechamentoAtual
        };
    }
}