using AutoMapper;
using Holiday.Api.Contract.Dto;
using Holiday.Api.Repository.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Holiday.Api.Core.Mappers;

public class InvitationMapperProfile : Profile
{
    public InvitationMapperProfile()
    {
        CreateMap<InvitationInDto, Invitation>()
            .ForMember(dest => dest.HolidayId, opt => opt.MapFrom(src => Guid.Parse(src.HolidayId)));
        
        CreateMap<Invitation, InvitationOutDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        
        CreateMap<InvitationOutDto, Invitation>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)));
    }
}