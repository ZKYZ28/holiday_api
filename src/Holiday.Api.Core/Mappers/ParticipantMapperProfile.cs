using AutoMapper;
using Holiday.Api.Repository.Models;

namespace DefaultNamespace;

public class ParticipantMapperProfile : Profile
{
    
    public ParticipantMapperProfile() 
    {
        CreateMap<ParticipantInDto, Participant>().ReverseMap(); 
        CreateMap<Participant, ParticipantOutDto>().ReverseMap(); 
    }
} 