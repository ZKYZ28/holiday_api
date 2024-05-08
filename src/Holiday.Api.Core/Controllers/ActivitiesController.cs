using System.Collections;
using AutoMapper;
using DefaultNamespace;
using Holiday.Api.Core.Utilities;
using Holiday.Api.Repository.CustomErrors;
using Holiday.Api.Repository.Models;
using Holiday.Api.Repository.Models.Services.Interface;
using Holiday.Api.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Holiday.Api.Core.Controllers;

[Authorize]
[ApiController]
[Route("/v1/activities")]
public class ActivitiesController : Controller
{
    private readonly IMapper _mapper;
    private readonly ILogger<ActivitiesController> _logger;
    private readonly IActivityRepository _activityRepository;
    private readonly IPictureService _pictureService;
    private readonly IParticipateRepository _participateRepository;
    private readonly IParticipantRepository _participantRepository;

    /// <summary>
    /// Initialise une nouvelle instance de la classe ActivityController.
    /// </summary>
    /// <param name="mapper">Effectue la mise en correspondance entre différents types d'objets.</param>
    /// <param name="logger">Journalisation utilisé pour enregistrer des messages et des informations de journalisation.</param>
    /// <param name="activityRepository">Interface qui permet l'accès aux données d'activités dans la base de données.</param>
    /// <param name="participantRepository">Interface qui permet l'accès aux données des participants dans la base de données.</param>
    /// <param name="participateRepository">Interface qui permet l'accès aux données de participants dans la base de données.</param>
    /// <param name="webHostEnvironment">Interface qui expose des informations différentes en fonction de si on se trouve en développement ou en production.</param>
    public ActivitiesController([FromServices] IMapper mapper, ILogger<ActivitiesController> logger, [FromServices] IParticipantRepository participantRepository, [FromServices] IActivityRepository activityRepository,[FromServices] IParticipateRepository participateRepository, [FromServices] IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _mapper = mapper;
        _activityRepository = activityRepository;
        _participateRepository = participateRepository;
        _pictureService = new PictureManager(webHostEnvironment.WebRootPath);
        _participantRepository = participantRepository;
    }
    
    /// <summary>
    /// Crée une nouvelle activité pour une vacances spécifique.
    /// </summary>
    /// <param name="activityInDto">Les données de l'activité à créer.</param>
    /// <param name="cancellationToken">Jeton d'annulation pour les opérations asynchrones.</param>
    /// <returns>
    /// - IActionResult représentant le résultat de la création de l'activité.
    /// - StatusCode 400 (BadRequest) si l'ajout de l'activité échoue.
    /// - StatusCode 200 (Ok) si l'activité est ajoutée avec succès
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateActivity([FromForm] ActivityInDto activityInDto, CancellationToken cancellationToken)
    {
        try
        {
            string? pathPicture = null;
            // Traitement de l'image reçue
            if (activityInDto.UploadedActivityPicture != null)
            {
                pathPicture = _pictureService.UploadFile(activityInDto.UploadedActivityPicture);
            }

            var activityEntity = _mapper.Map<Activity>(activityInDto);
            
            if (pathPicture != null)
            {
                activityEntity.ActivityPath = pathPicture;
            }

            if (!await LocationValidator.IsAddressValidAsync(activityEntity.Location.GetFormattedAddress()))
            {
                _logger.LogError("L'adresse de l'activité fournie n'est pas valide.");
                return BadRequest("L'adresse fournie n'est pas valide.");
            }

            if (!await _activityRepository.AddActivity(activityEntity, cancellationToken))
            {
                _logger.LogError("Une erreur est survenue lors de l'ajout de l'activité dans la base de données.");
                return BadRequest("Échec de l'ajout de l'activité.");
            }

            _logger.LogInformation("L'activité {ActivityName} a été ajoutée avec succès.", activityEntity.Name);
            return Ok("Activité ajoutée avec succès.");
        }
        catch (HolidayStorageException)
        {
            _logger.LogError("Une erreur est survenue lors de l'ajout de l'activité.");
            return BadRequest("Un problème est survenu lors de l'enregistrement de l'image.");
        }
        catch (LocationException ex)
        {
            _logger.LogError("Une erreur est survenue lors de l'ajout de l'activité.");
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Met à jour une activité spécifique en fonction de l'identifiant de celle-ci.
    /// </summary>
    /// <param name="activityId">L'id de l'activité que l'on souhaite mettre à jour.</param>
    /// <param name="activityInDto">Les données de l'activité à mettre à jour.</param>
    /// <param name="cancellationToken">Jeton d'annulation pour les opérations asynchrones.</param>
    /// <returns>
    /// - IActionResult représentant le résultat de la création de l'activité.
    /// - StatusCode 400 (BadRequest) si l'ajout de l'activité échoue.
    /// - StatusCode 200 (Ok) si l'activité est mise à jour avec succès
    /// </returns>
    [HttpPut]
    [Route("{activityId}")]
    public async Task<IActionResult> UpdateActivity([FromRoute] string activityId, [FromForm] ActivityEditInDto activityInDto, CancellationToken cancellationToken)
    {
        DeleteOldImage(activityInDto);
        
        if (!VerifyCorrectionImage(activityInDto, out var pathPicture, out var actionResult)) return actionResult;

        var activityEntity = _mapper.Map<Activity>(activityInDto);
        activityEntity.Id = new Guid(activityId);
        
        if (pathPicture != null)
        {
            activityEntity.ActivityPath = pathPicture;   
        }
        
        if (!await LocationValidator.IsAddressValidAsync(activityEntity.Location.GetFormattedAddress()))
        {
            _logger.LogError("L'adresse de l'activité fournie n'est pas valide.");
            return BadRequest("L'adresse fournie n'est pas valide.");
        }
        
        if (activityInDto.DeleteImage == false && pathPicture == null)
        {
            activityEntity.ActivityPath = activityInDto.InitialPath;
        }
    
        if (await _activityRepository.UpdateActivity(activityEntity, cancellationToken))
        {
            _logger.LogInformation("L'activité {ActivityName} a été mise à jour avec succès.", activityEntity.Name);
            return Ok("Votre activité a été mis à jour avec succès.");
        }
    
        _logger.LogError("Une erreur est survenue lors de la mise à jour de l'activité.");
        return BadRequest("Votre activity n'a pas pu être mise à jour.");
    }

    private void DeleteOldImage(ActivityEditInDto activityInDto)
    {
        if (activityInDto.DeleteImage == true)
        {
            try
            {
                _pictureService.deletePicture(activityInDto.InitialPath);
            }
            catch (HolidayStorageException ex)
            {
                _logger.LogError("Une erreur s'est produite lors de la suppression de l'ancienne image.");
                _logger.LogError(ex, "Une exception s'est produite lors de la suppression de l'ancienne image.");
            }
        }
    }

    private bool VerifyCorrectionImage(ActivityEditInDto activityInDto, out string? pathPicture,
        out IActionResult actionResult)
    {
        pathPicture = null;
        actionResult = null;
        // Traitement de l'image reçue
        if (activityInDto.UploadedActivityPicture != null)
        {
            try
            {
                pathPicture = _pictureService.UploadFile(activityInDto.UploadedActivityPicture);
            }
            catch (HolidayStorageException)
            {
                {
                    _logger.LogError("Une erreur s'est produite lors de l'enregistrement de l'image.");
                    actionResult = BadRequest("Un problème est survenu lors de l'enregistrement de l'image.");
                    return false;
                }
            }
        }
        return true;
    }


    /// <summary>
    /// Récupère une activité spécifique sur base de son identifiant.
    /// </summary>
    /// <param name="activityId">L'identifiant de l'activité à récupérer.</param>
    /// <returns>
    /// - Code 200 (Ok) si l'activité a été récupérée correctement.
    /// - Code 400 (Bad Request) avec un message d'erreur en cas d'échec de la récupération de l'activité.
    /// </returns>
    [HttpGet]
    [Route("{activityId}")]
    public async Task<IActionResult> GetActivityById([FromRoute] string activityId)
    {
        try
        {
            var activity = await _activityRepository.GetActivityById(new Guid(activityId));
            var activityOutDto = _mapper.Map<ActivityOutDto>(activity);
            
            _logger.LogInformation("L'activité ayant l'identifiant {ActivityId} a été chargée avec succès.", activityId);
            return Ok(activityOutDto);
        }
        catch (LoadDataBaseException e)
        {
            _logger.LogError("Une erreur est survenue lors du chargement de l'activité.");
            return BadRequest("Erreur lors de la récupération de l'activité.");
        }
    }
    
    /// <summary>
    /// Supprime une activité spécifiée en fonction de son identifiant.
    /// </summary>
    /// <param name="activityId">L'ID de l'activité à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation pour les opérations asynchrones.</param>
    /// <returns>
    /// - Code 200 (Ok) si la suppression de l'activité réussit.
    /// - Code 400 (Bad Request) avec un message d'erreur en cas d'échec de la suppression.
    /// </returns>
    [HttpDelete]
    [Route("{activityId}")]
    public async Task<IActionResult> DeleteActivity([FromRoute] string activityId, CancellationToken cancellationToken)
    {
        if (!await _activityRepository.DeleteActivity(new Guid(activityId), cancellationToken))
        {
            _logger.LogError("Une erreur est survenue lors de la suppression de l'activité {ActivityId}.", activityId);
            return BadRequest("Échec de la supression de l'activité.");
        }
        
        _logger.LogInformation("L'activité ayant l'id {ActivityId} a été supprimée avec succès.", activityId);
        return Ok("Suppression faite avec succès.");
    }
    
    /// <summary>
    /// Récupérer la liste de toutes les participants liés à une activité spécifique en fonction de l'identifiant de celle-ci.
    /// </summary>
    /// <param name="activityId">L'identifiant de l'activité pour laquelle on souhaite récupérer les participants liés.</param>
    /// <param name="isParticipated">Booléen indiquant si l'on souhaite récupérer les participants qui sont membres ou non de l'activité.</param>
    /// <param name="cancellationToken">Jeton d'annulation pour les opérations asynchrones.</param>
    /// <returns>
    /// - Code 200 (Ok) La liste des participations liées à l'activité spécifique..
    /// - Code 400 (Bad Request) avec un message d'erreur en cas d'échec lors de la récupération.
    /// </returns>
    [HttpGet("{activityId}/participants")]
    public async Task<IActionResult> GetParticipantsByActivity([FromRoute] string activityId, [FromQuery] bool isParticipated, CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable participants = new List<Participant>();
            
            if (isParticipated)
            {
                participants =
                    await _participateRepository.GetAllParticipantsByActivity(new Guid(activityId), cancellationToken);
            }
            else
            {
                participants =
                    await _participantRepository.GetParticipantsNotYetInActivity(new Guid(activityId), cancellationToken);
            }
          
            var participantsOutDto = _mapper.Map<ICollection<ParticipantOutDto>>(participants);

            _logger.LogInformation("Récupération de tous les participants de l'activité {ActivityId}.", activityId);
            return Ok(participantsOutDto);
        }
        catch (LoadDataBaseException ex)
        {
            _logger.LogError("Erreur en base de données lors de la récupération de tous les participants de l'activité {ActivityId}.", activityId);
            return BadRequest(ex.Message);
        }
        catch (RessourceNotFoundException ex)
        {
            _logger.LogError("Erreur en base de données lors de la récupération de tous les participants de l'activité {ActivityId}.", activityId);
            return BadRequest(ex.Message);
        }
    }
        
        
    //PARTICIPATE 
    
    /// <summary>
    /// Supprimer une participation sur base de l'identifiant de l'activité et l'identifiant du participant.
    /// </summary>
    /// <param name="activityId">L'id du l'activité à laquelle on souhaite supprimer la participation</param>
    /// <param name="participantId">L'id du participant auquel on souhaite supprimer la participation</param>
    /// <param name="cancellationToken">Jeton d'annulation pour les opérations asynchrones.</param>
    /// <returns>
    /// - Code 200 (Ok) un message confirmant la suppression.
    /// - Code 400 (Bad Request) avec un message d'erreur en cas d'échec lors de la suppression.
    /// </returns>
    [HttpDelete]
    [Route("{activityId}/participants/{participantId}")]
    public async Task<IActionResult> DeleteParticipate([FromRoute] string activityId, [FromRoute] string participantId, CancellationToken cancellationToken)
    {
        if (!await _participateRepository.RemoveParticipate(new Guid(activityId), participantId, cancellationToken))
        {
            _logger.LogError("Erreur en base de données lors de la suppression du participant {ParticipantId} à l'activité {ActivityId}.", participantId, activityId);
            return BadRequest("Votre participation n'a pas pu être supprimée.");
        }
        
        _logger.LogError("Erreur en base de données lors de la suppression du participant {ParticipantId} à l'activité {ActivityId}.", participantId, activityId);
        return Ok( "Participation supprimée avec succès.");
    }
    
    /// <summary>
    /// Crée une participation sur base de l'identifiant de l'activité et l'identifiant du participant .
    /// </summary>
    /// <param name="activityId">L'id du l'activité à laquelle on souhaite ajouter la participation</param>
    /// <param name="participantId">L'id du participant auquel on souhaite ajouter la participation</param>
    /// <param name="cancellationToken">Jeton d'annulation pour les opérations asynchrones.</param>
    /// <returns>
    /// - Code 200 (Ok) un message confirmant la création.
    /// - Code 400 (Bad Request) avec un message d'erreur en cas d'échec lors de la création.
    /// </returns>
    [HttpPost]
    [Route("{activityId}/participants/{participantId}")]
    public async Task<IActionResult> CreateParticipate([FromRoute] string activityId, [FromRoute] string participantId, CancellationToken cancellationToken)
    {
        var newParticipate = new Participate
        {
            ActivityId = new Guid(activityId),
            ParticipantId = participantId
        };

        if (!await _participateRepository.AddParticipate(newParticipate, cancellationToken))
        {
            _logger.LogError("La participation n'a pas pû être ajoutée.");
            return BadRequest("La participant n'a pas pû être ajoutée à l'activité.");
        }
        
        _logger.LogInformation("La participation a été ajoutée avec succès.");
        return Ok( "Participation ajoutée avec succès");
    }
        
}