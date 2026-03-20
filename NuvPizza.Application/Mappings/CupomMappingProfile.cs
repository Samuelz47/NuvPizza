using AutoMapper;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;
using Org.BouncyCastle.Asn1.Cmp;

namespace NuvPizza.Application.Mappings;

public class CupomMappingProfile : Profile
{
    public CupomMappingProfile()
    {
        CreateMap<CupomForRegistrationDTO, Cupom>();

        CreateMap<Cupom, CupomDTO>();
    }

}