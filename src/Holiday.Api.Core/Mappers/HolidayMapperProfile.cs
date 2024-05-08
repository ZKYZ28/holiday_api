using AutoMapper;
using Holiday.Api.Contract.Dto;
using Holiday.Api.Repository.Models;

namespace Holiday.Api.Core.Mappers;

public class HolidayMapperProfile : Profile
{
    public HolidayMapperProfile()
    {
        CreateMap<HolidayInDto, Repository.Models.Holiday>();
        CreateMap<HolidayEditInDto, Repository.Models.Holiday>();
        CreateMap<Repository.Models.Holiday, Repository.Models.Holiday>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Ignore l'ID pour éviter les modifications accidentelles

        CreateMap<Repository.Models.Holiday, HolidayOutDto>();
        CreateMap<HolidayOutDto, Repository.Models.Holiday>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)));
        
        CreateMap<NewParticipantDto, Participant>();
    }
}