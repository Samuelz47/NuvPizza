using AutoMapper;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Mappings;

public class ItemPedidoMappingProfile : Profile
{
    public ItemPedidoMappingProfile()
    {
        CreateMap<ItemPedidoForRegistrationDTO, ItemPedido>();
        CreateMap<ItemPedidoComboEscolhaForRegistrationDTO, ItemPedidoComboEscolha>();
        
        CreateMap<ItemPedido, ItemPedidoDTO>()
            .ForMember(dest => 
                                        dest.NomeProduto, opt => 
                                            opt.MapFrom(src => src.Nome));
        CreateMap<ItemPedidoComboEscolha, ItemPedidoComboEscolhaForRegistrationDTO>();
    }
}