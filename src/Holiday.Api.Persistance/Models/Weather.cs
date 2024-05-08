namespace Holiday.Api.Repository.Models;

public class Weather
{
    public Weather(WeatherDay currentDay, ICollection<WeatherDay> weatherDays)
    {
        CurrentDay = currentDay;
        WeatherDays = weatherDays;
    }
    
    public WeatherDay CurrentDay { get; set; }
    
    public ICollection<WeatherDay> WeatherDays { get; set;}
    
    
    public void KeepOnlyHolidayDays(DateTimeOffset startDate)
    {
      List<WeatherDay> tempList = WeatherDays.ToList();
      
      if (startDate > DateTime.Now)
      {
          TimeSpan difference =  startDate - DateTime.Now;
          int differenceInDays = (int)difference.TotalDays;
          tempList.RemoveRange(0, differenceInDays + 1);
      }
      
      this.WeatherDays = tempList;
    } 
}