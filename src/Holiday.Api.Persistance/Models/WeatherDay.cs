namespace Holiday.Api.Repository.Models;

public class WeatherDay
{
    public WeatherDay(DateTimeOffset date, float maxTemp, float minTemp, float currentTemp, float riskOfRain, float riskOfSnow, WeatherCondition condition, ICollection<WeatherHour> weatherByHour)
    {
        Date = date;
        MaxTemp = maxTemp;
        MinTemp = minTemp;
        CurrentTemp = currentTemp;
        RiskOfRain = riskOfRain;
        RiskOfSnow = riskOfSnow;
        Condition = condition;
        WeatherByHour = weatherByHour;
    }
    
    public DateTimeOffset Date { get; set;}
    public float MaxTemp { get; set; }
    public float MinTemp { get; set; }
    public float CurrentTemp { get; set; }
    public float RiskOfRain { get; set; }
    public float RiskOfSnow { get; set; }
    public WeatherCondition Condition { get; set; }
    public ICollection<WeatherHour> WeatherByHour { get; set;}
}