using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Services;

public class PagamentoDummyService : IPagamentoService
{
    public async Task<string> GerarQrCodePix(Pedido pedido)
    {
        throw new NotImplementedException();
    }
}