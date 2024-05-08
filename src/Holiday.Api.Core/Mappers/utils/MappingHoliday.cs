using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Holiday.Api.Core.Mappers.utils;

public static class HolidayMapping
{
    
    public static Repository.Models.Holiday MapUpdatedHolidayToExistingHoliday([FromServices] IMapper mapper, Repository.Models.Holiday updatedHoliday, Repository.Models.Holiday existingHoliday)
    {
        return mapper.Map(updatedHoliday, existingHoliday);
    }
}
