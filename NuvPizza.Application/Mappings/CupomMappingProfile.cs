using AutoMapper;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Mappings;

public class CupomMappingProfile : Profile
{
    public CupomMappingProfile()
    {
        CreateMap<CupomForRegistrationDTO, Cupom>()
            .ForMember(dest => dest.PedidoMinimo, opt => opt.MapFrom(src => src.PedidoMinimo));

        CreateMap<Cupom, CupomDTO>()
            .ForMember(dest => dest.PedidoMinimo, opt => opt.MapFrom(src => src.PedidoMinimo));
    }
}