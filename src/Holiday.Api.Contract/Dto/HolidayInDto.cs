using System.Diagnostics;
using DefaultNamespace;
using Microsoft.AspNetCore.Http;

namespace Holiday.Api.Contract.Dto;

public class HolidayInDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public LocationDto Location { get; set; }
    
    public IFormFile? UploadedHolidayPicture { get; set; }

    public string CreatorId { get; set;}
    
}

public class HolidayEditInDto : HolidayInDto
{
    public bool? DeleteImage { get; set; }
    public string InitialPath { get; set; }
    
    public bool IsPublish { get; set; }
    
}

public class HolidayOutDto : HolidayInDto
{
    public string Id { get; set; }

    public ICollection<ParticipantOutDto>? Participants { get; set;}
    
    public string? HolidayPath { get; set; }
    
    public bool IsPublish { get; set; }

    public ICollection<ActivityOutDto> Activities { get; set; }
}