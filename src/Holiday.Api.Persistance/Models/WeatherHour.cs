namespace Holiday.Api.Repository.Models;

public class WeatherHour
{
    public WeatherHour(DateTimeOffset dateAndTime, float temp, string pathImage)
    {
        this.dateAndTime = dateAndTime;
        this.temp = temp;
        this.pathImage = pathImage;
    }
    
    public DateTimeOffset dateAndTime { get; set;}
    public float temp { get; set; } 
    public string pathImage { get; set; }
}