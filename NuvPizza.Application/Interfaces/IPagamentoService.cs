using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Interfaces;

public interface IPagamentoService
{
    Task<string> GerarQrCodePix(Pedido pedido);
}