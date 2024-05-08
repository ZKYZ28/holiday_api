using AutoMapper;
using DefaultNamespace;
using Holiday.Api.Contract.Dto;

namespace Holiday.Api.Core.Mappers;

public class ChatMapperProfile : Profile
{

    public ChatMapperProfile()
    {
        CreateMap<ChatDto, Repository.Models.Message>().ReverseMap();
    }
}