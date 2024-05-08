using AutoMapper;
using Holiday.Api.Contract.Dto;
using Holiday.Api.Repository.Models;

namespace Holiday.Api.Core.Mappers;

public class StatisticsMapperProfile : Profile
{
    public StatisticsMapperProfile()
    {
        CreateMap<Repository.Models.Statistics, StatisticsDto>();
    }
}