using Microsoft.AspNetCore.Http;

namespace DefaultNamespace;

public class ActivityInDto
{
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public float Price { get; set; }
    
    public IFormFile? UploadedActivityPicture { get; set; }
    
    public DateTimeOffset StartDate { get; set; }
    
    public DateTimeOffset EndDate { get; set; }
    
    public LocationDto Location { get; set; }
    public string HolidayId { get; set; }
   
}

public class ActivityEditInDto : ActivityInDto
{
    public bool? DeleteImage { get; set; }
    public string InitialPath { get; set; }
}


public class ActivityOutDto : ActivityInDto
{
    public string Id { get; set; }
    
    public string? ActivityPath { get; set; }
}