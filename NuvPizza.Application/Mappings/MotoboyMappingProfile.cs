using AutoMapper;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Mappings
{
    public class MotoboyMappingProfile : Profile
    {
        public MotoboyMappingProfile()
        {
            CreateMap<Motoboy, MotoboyDTO>();
            CreateMap<MotoboyCreateDTO, Motoboy>();
            CreateMap<MotoboyUpdateDTO, Motoboy>();
        }
    }
}
