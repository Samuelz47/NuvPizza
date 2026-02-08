using AutoMapper;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Enums;

namespace NuvPizza.Application.Mappings;

public class PedidoMappingProfile : Profile
{
    public PedidoMappingProfile()
    {
        CreateMap<PedidoForRegistrationDTO, Pedido>()
            .ForMember(dest => dest.FormaPagamento, opt => opt.MapFrom(src =>
                src.FormaPagamento == "Pagar na Entrega"
                    ? FormaPagamento.CartaoEntrega // Traduz o texto especial
                    : Enum.Parse<FormaPagamento>(src.FormaPagamento, true)
             ));
        
        CreateMap<Pedido, PedidoDTO>();
    }
}