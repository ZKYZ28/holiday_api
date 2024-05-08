using AutoMapper;
using Holiday.Api.Repository.Models;

namespace DefaultNamespace;

public class LocationMapperProfile : Profile
{

    public LocationMapperProfile()
    {
        CreateMap<LocationDto, Location>().ReverseMap();
    }
}