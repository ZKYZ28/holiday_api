using Holiday.Api.Repository.Models;
using Newtonsoft.Json.Linq;

namespace Holiday.Api.Core.Utilities;

public class ParserWeather
{
    private JObject _weatherData;
    private readonly DateTimeOffset _startDate;
    private Weather _weather;
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe ParserWeather en analysant les données météorologiques JSON.
    /// </summary>
    /// <param name="jsonWeatherData">Les données météorologiques au format JSON.</param>
    /// <param name="startDate">La date de début des vacances à partir de laquelle on veut les informations météorologiques</param>
    /// <exception cref="ArgumentException">Se produit en cas d'erreur de désérialisation JSON.</exception>
    public ParserWeather(string jsonWeatherData, DateTimeOffset startDate)
    {
        try
        {
            this._weatherData = JObject.Parse(jsonWeatherData);
            this._startDate = startDate;
            this._weather = new Weather(null, new List<WeatherDay>());
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            throw new ArgumentException("Erreur lors de la désérialisation du JSON", ex);
        }
    }
    
    /// <summary>
    /// Obtient les données météorologiques au format JSON.
    /// </summary>
    public JObject WeatherDataJson
    {
        get { return _weatherData; }
    }
    
    /// <summary>
    /// Analyse les données météorologiques au format JSON pour créer un objet Weather contenant les prévisions météorologiques.
    /// </summary>
    /// <returns>
    /// Un objet Weather contenant les prévisions météorologiques, y compris les données du jour actuel et les prévisions pour les jours suivants.
    /// </returns>
    public Weather ParseJsonToWeatherObject()
    {
        CheckForAddCurrentDay();
        AddAllDay();
        return _weather;
    }

    /// <summary>
    /// Vérifie si la date actuelle est supérieure ou égale à la date de début des vacances.
    /// Si c'est le cas, ajoute les données météorologiques du jour actuel à l'objet Weather.
    /// </summary>
    private void CheckForAddCurrentDay()
    {
        if (DateTime.Now >= _startDate)
        {
            _weather.CurrentDay = new WeatherDay(
                    (DateTime)_weatherData["current"]["last_updated"],
                    (float)_weatherData["forecast"]["forecastday"][0]["day"]["maxtemp_c"],
                    (float)_weatherData["forecast"]["forecastday"][0]["day"]["mintemp_c"],
                    (float)_weatherData["current"]["temp_c"],
                    (float)_weatherData["forecast"]["forecastday"][0]["day"]["daily_chance_of_rain"],
                    (float)_weatherData["forecast"]["forecastday"][0]["day"]["daily_chance_of_snow"],
                    new WeatherCondition (
                        (string)_weatherData["current"]["condition"]["text"],
                        (string)_weatherData["current"]["condition"]["icon"]
                    ),
                    new List<WeatherHour>()
                )
            ;
        }
    }
    
    /// <summary>
    /// Ajoute les données météorologiques pour chaque jour à l'objet Weather en parcourant le tableau JSON des jours.
    /// </summary>
    private void AddAllDay()
    {
        JArray forecastDayArray = (JArray)_weatherData["forecast"]["forecastday"];
        
        foreach (JToken forecastDayToken in forecastDayArray)
        {
            JObject forecastDayObject = (JObject)forecastDayToken;
            JObject dayObject = (JObject)forecastDayObject["day"];
            JObject conditionObject = (JObject)dayObject["condition"];
            JArray hoursArray = (JArray)forecastDayObject["hour"];
            
            _weather.WeatherDays.Add(new WeatherDay(
                (DateTime)forecastDayObject["date"],
                (float)dayObject["maxtemp_c"],
                (float)dayObject["mintemp_c"],
                0, 
                (float)dayObject["daily_chance_of_rain"],
                (float)dayObject["daily_chance_of_rain"],
                new WeatherCondition(
                    (string)conditionObject["text"],
                    (string)conditionObject["icon"]
                ),
                GetAllHoursForOneDay(hoursArray)
                ));
        }
    }

    /// <summary>
    /// Récupère toutes les données météorologiques horaires pour une journée donnée à partir d'un tableau JSON.
    /// </summary>
    /// <param name="hoursArray">Le tableau JSON contenant les données météorologiques horaires pour une journée.</param>
    /// <returns>
    /// Une liste d'objets WeatherHour représentant les données météorologiques horaires pour la journée.
    /// Chaque objet WeatherHour contient l'heure, la température en degrés Celsius et l'icône de condition météorologique.
    /// </returns>
    private List<WeatherHour> GetAllHoursForOneDay(JArray hoursArray)
    {
        List<WeatherHour> listWeatherHours = new List<WeatherHour>();
        
        foreach (JToken hourToken in hoursArray)
        {
            JObject hourObject = (JObject)hourToken;

            listWeatherHours.Add(new WeatherHour(
                (DateTime)hourObject["time"],
                (float)hourObject["temp_c"],
                (string)hourObject["condition"]["icon"]
                ));
        }

        return listWeatherHours;
    }
}