using AutoMapper;
using Holiday.Api.Contract.Dto;
using Holiday.Api.Repository.Models;

namespace Holiday.Api.Core.Mappers;

public class ParticipateMapperProfile : Profile
{
    public ParticipateMapperProfile()
    {
        CreateMap<ParticipateInDto, Participate>()
            .ForMember(dest => dest.ActivityId, opt => opt.MapFrom(src => Guid.Parse(src.ActivityId)));
        
        CreateMap<Participate, ParticipateOutDto>().ReverseMap();
        CreateMap<ParticipateOutDto, Participate>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)));
    }
}