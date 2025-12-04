using AutoMapper;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Mappings;

public class PedidoMappingProfile : Profile
{
    public PedidoMappingProfile()
    {
        CreateMap<PedidoForRegistrationDTO, Pedido>();
        
        CreateMap<Pedido, PedidoDTO>();
    }
}