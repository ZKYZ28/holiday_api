using AutoMapper;
using Holiday.Api.Repository.CustomErrors;
using Holiday.Api.Repository.Models;
using Holiday.Api.Repository.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Holiday.Api.Core.Controllers;

[ApiController]
[Route("/v1/statistics")]
public class StatisticsController : Controller
{
    
    private readonly IMapper _mapper;
    private readonly ILogger<ActivitiesController> _logger;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IParticipantRepository _participantRepository;

    /// <summary>
    /// Initialise une nouvelle instance de la classe StatisticsController.
    /// </summary>
    /// <param name="mapper">Effectue la mise en correspondance entre différents types d'objets.</param>
    /// <param name="logger">Journalisation utilisé pour enregistrer des messages et des informations de journalisation.</param>
    /// <param name="participantRepository">Interface qui permet l'accès aux données des participants dans la base de données.</param>
    /// <param name="holidayRepository">Interface qui permet l'accès aux données des vacances dans la base de données.</param>
    public StatisticsController([FromServices] IMapper mapper, ILogger<ActivitiesController> logger, [FromServices] IParticipantRepository participantRepository, [FromServices] IHolidayRepository holidayRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _holidayRepository = holidayRepository;
        _participantRepository = participantRepository;
    }
    
    /// <summary>
    /// Récupère le nombre de participants en vacances par pays pour une date spécifique.
    /// </summary>
    /// <param name="date">La date pour laquelle on souhaite récupérer le nombre de participants en vacances</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec un objet Statistics comportant le nombre de participants en vacances par pays pour la date spécifiée.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur si un problème survient lors de la récupération du nombre de participants en vacances par pays.
    /// </returns>
    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetStatisticsForDate([FromRoute] DateTimeOffset date)
    {
        try
        {
            var statisticsList = await _holidayRepository.GetCountHolidaysForDate(date);
            var statisticsListDto = _mapper.Map<ICollection<Statistics>>(statisticsList);
            
            _logger.LogInformation("Récupération du nombre de participants en vacances pour une date donnée.");
            return Ok(statisticsListDto);
        }
        catch(LoadDataBaseException ex)
        {
            _logger.LogError("Une erreur est survenue lors de la récupération du nombre de participants en vacances pour une date donnée.");
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Récupère le nombre total de participants enregistrés dans l'application.
    /// </summary>
    /// <returns>
    /// - StatusCode 200 (OK) avec le nombre total de participants si la récupération réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur si un problème survient lors de la récupération du nombre de participants.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var participantCount =  _participantRepository.GetParticipantCount();
            Statistics myStatistics = new Statistics
            {
                ActiveParticipants = participantCount
            };
            
            _logger.LogInformation("Nombre de participants récupérés avec succès.");
            return Ok(myStatistics);
        }
        catch (LoadDataBaseException ex)
        {
            _logger.LogError("Erreur dans la base de données lors de la récupération du nombre de participants.");
            return BadRequest(ex.Message);
        }
    }
}