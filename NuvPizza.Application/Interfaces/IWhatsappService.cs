using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Interfaces;

public interface IWhatsappService
{
    string GerarLinkPedido(Pedido pedido);
}