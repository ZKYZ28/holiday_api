using AutoMapper;
using Holiday.Api.Repository.Models;

namespace DefaultNamespace;

public class ActivityMapperProfile : Profile
{

    public ActivityMapperProfile()
    {
        CreateMap<ActivityInDto, Activity>()
            .ForMember(dest => dest.HolidayId, opt => opt.MapFrom(src => Guid.Parse(src.HolidayId)));
        CreateMap<ActivityEditInDto, Activity>()
            .ForMember(dest => dest.HolidayId, opt => opt.MapFrom(src => Guid.Parse(src.HolidayId)));
        CreateMap<Activity, ActivityOutDto>();
        CreateMap<ActivityOutDto, Activity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(dest => dest.HolidayId, opt => opt.MapFrom(src => Guid.Parse(src.HolidayId)));
    }
}