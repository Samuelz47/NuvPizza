using AutoMapper;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Mappings;

public class ProdutoMappingProfile : Profile
{
    public ProdutoMappingProfile()
    {
        CreateMap<ProdutoForRegistrationDTO, Produto>();
        
        CreateMap<Produto, ProdutoDTO>();
    }
}