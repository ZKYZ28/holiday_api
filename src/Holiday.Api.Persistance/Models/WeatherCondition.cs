namespace Holiday.Api.Repository.Models;

public class WeatherCondition
{
    public WeatherCondition(string description, string iconPath)
    {
        Description = description;
        IconPath = iconPath;
    }
    
    public string Description { get; set; }
    public string IconPath { get; set; }
}