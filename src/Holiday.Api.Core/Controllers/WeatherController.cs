using AutoMapper;
using Holiday.Api.Core.Utilities;
using Holiday.Api.Repository.CustomErrors;
using Holiday.Api.Repository.Models;
using Holiday.Api.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Holiday.Api.Core.Controllers;

[ApiController]
[Authorize]
[Route("/v1/weather")]
public class WeatherController : Controller
{
    private readonly ILogger<ActivitiesController> _logger;
    private readonly IHolidayRepository _holidayRepository;
    private const string WeatherApiKey = "cbc6c6b0322547c4b84123859231010";
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe WeatherController.
    /// </summary>
    /// <param name="logger">Le journal (logger) utilisé pour l'enregistrement des messages de journalisation.</param>
    /// <param name="holidayRepository">Interface qui permet l'accès aux données des vacances.</param>
    public WeatherController(ILogger<ActivitiesController> logger, [FromServices] IHolidayRepository holidayRepository)
    {
        _logger = logger;
        _holidayRepository = holidayRepository;
    }
    
    /// <summary>
    /// Renvoie les prévisions météorologiques pour une vacances donnée sur base de son identifiant.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la vacances.</param>
    /// <returns>
    /// - StatusCode 200 (OK) Un objet Weather contenant les prévisions météorologiques pour la destination de la vacances.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur si un problème survient lors de la récupération des données météorologiques.
    /// </returns>
    [HttpGet("{holidayId}")]
    public async Task<IActionResult> GetWeatherForecast([FromRoute] string holidayId)
    {
        Repository.Models.Holiday holiday = null;
        try
        {
            holiday = await _holidayRepository.GetHolidayById(new Guid(holidayId));

            if (!holiday.CheckIfDateIsValid(holiday.StartDate))
            {
                _logger.LogError("La date de départ est trop éloignée pour que nous puissions récupérer les données météo.");
                return BadRequest("La date de départ est trop éloignée pour que nous puissions récupérer les données météo.");
                
            }

            if (!CountryCodeDictionary.COUNTRY_CODES.ContainsKey(holiday.Location!.Country.ToUpper()))
            {
                _logger.LogError("Impossible de récupérer les informations météorologiques. Le pays '{HolidayCountry}' de votre séjour pourrait être inconnu de nos services.", holiday.Location.Country);
                return BadRequest($"Impossible de récupérer les informations météorologiques. Le pays '{holiday.Location.Country}' de votre séjour pourrait être inconnu de nos services.");
            }

            var weatherData = await GetWeatherData(holiday.Location.Locality, CountryCodeDictionary.COUNTRY_CODES[holiday.Location.Country.Trim().ToUpper()]);

            if (weatherData == null)
            {
                _logger.LogError("Impossible de récupérer les informations météorologiques.  La localité {HolidayLocality} de votre séjour pourrait être inconnu de nos services.", holiday.Location.Locality);
                return BadRequest($"Impossible de récupérer les informations météorologiques.  La localité '{holiday.Location.Locality}' de votre séjour pourrait être inconnu de nos services.");
            }
            
            ParserWeather parserWeather = new ParserWeather(weatherData, holiday.StartDate);

            if (parserWeather.WeatherDataJson != null)
            {
                Weather weather = parserWeather.ParseJsonToWeatherObject();
                weather.KeepOnlyHolidayDays(holiday.StartDate);
                
                _logger.LogInformation("Récupération de données météorologiques effectuée avec succès.");
                return Ok(weather);
            }
           
            _logger.LogError("Aucune donnée météorologique disponible pour cette destination.");
            return BadRequest("Aucune donnée météorologique disponible pour cette destination.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Erreur lors de la récupération des données météorologiques.");
            return BadRequest("Erreur lors de la récupération des données météorologiques.");
        }
    }


    /// <summary>
    /// Récupère les données météorologiques pour une ville donnée et un code de pays donné en utilisant une API météorologique (Weather API).
    /// </summary>
    /// <param name="city">Le nom de la ville pour laquelle récupérer les données météorologiques.</param>
    /// <param name="countryCode">Le code de pays associé à la ville.</param>
    /// <returns>
    /// Une chaîne de caractères représentant les données météorologiques au format JSON si la récupération réussit.
    /// Si une erreur se produit lors de la récupération, la méthode renvoie un message d'erreur.
    /// Si la requête vers l'API échoue ou si les données ne sont pas disponibles, la méthode renvoie null.
    /// </returns>
    private async Task<string> GetWeatherData(string city, string countryCode)
    {
        string apiUrl = $"http://api.weatherapi.com/v1/forecast.json?key={WeatherApiKey}&q={city},{countryCode}&days=7";
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erreur lors de la récupération des informations météo au près de weatherapi.");
                return null;
            }
        }
    }
}