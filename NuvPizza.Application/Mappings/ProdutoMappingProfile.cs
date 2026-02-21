using AutoMapper;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Mappings;

public class ProdutoMappingProfile : Profile
{
    public ProdutoMappingProfile()
    {
        CreateMap<ProdutoForRegistrationDTO, Produto>();
        CreateMap<ProdutoForUpdateDTO, Produto>();
        
        CreateMap<Produto, ProdutoDTO>()
            .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => (int)src.Categoria))
            .ForMember(dest => dest.Tamanho, opt => opt.MapFrom(src => (int)src.Tamanho));

        CreateMap<ComboItemTemplate, ComboItemTemplateDTO>().ReverseMap();
    }
}