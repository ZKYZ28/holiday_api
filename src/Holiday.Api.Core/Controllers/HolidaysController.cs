using System.Collections;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using DefaultNamespace;
using Holiday.Api.Contract.Dto;
using Holiday.Api.Core.Utilities;
using Holiday.Api.Repository.CustomErrors;
using Holiday.Api.Repository.Models;
using Holiday.Api.Repository.Models.Services.Interface;
using Holiday.Api.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HolidayModel = Holiday.Api.Repository.Models.Holiday;

namespace Holiday.Api.Core.Controllers;

[Authorize]
[ApiController]
[Route("/v1/holidays")]
public class HolidaysController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<HolidaysController> _logger;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IPictureService _pictureService;
    private readonly IParticipantRepository _participantRepository;
    private readonly IParticipateRepository _participateRepository ;
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe HolidayController.
    /// </summary>
    /// <param name="mapper">L'objet de mappage utilisé pour mapper des objets entre différents odèles.</param>
    /// <param name="logger">Le journal (logger) utilisé pour l'enregistrement des messages de journalisation.</param>
    /// <param name="participateRepository">Interface qui permet l'accès aux données des participations.</param>
    /// <param name="holidayRepository">Interface qui permet l'accès aux données des vacances.</param>
    /// <param name="invitationRepository">Interface qui permet l'accès aux données des invitations.</param>
    /// <param name="webHostEnvironment">Interface qui expose des informations différentes en fonction de si on se trouve en développement ou en production.</param>
    /// <param name="participantRepository">Interface qui permet l'accès aux données des participants.</param>
    public HolidaysController([FromServices] IMapper mapper, ILogger<HolidaysController> logger,[FromServices] IParticipateRepository participateRepository, [FromServices] IHolidayRepository holidayRepository,[FromServices] IInvitationRepository invitationRepository, [FromServices] IWebHostEnvironment webHostEnvironment, [FromServices] IParticipantRepository participantRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _holidayRepository = holidayRepository;
        _invitationRepository = invitationRepository;
        _participantRepository = participantRepository;
        _participateRepository = participateRepository;
        _pictureService = new PictureManager(webHostEnvironment.WebRootPath);
    }
    
    /// <summary>
    /// Récupère toutes les vacances associées à un participant spécifique en fonction de son identifiant.
    /// </summary>
    /// <param name="isPublished">Booléen qui permet d'indiquer si on veut les vacances publiées ou non</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec la liste des vacances si la récupération réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur dans les cas suivants :
    ///   - Aucune vacance n'a été trouvée pour le participant.
    ///   - Une exception est levée lors de la récupération des vacances.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAllHolidayByParticipant([FromQuery] bool isPublished)
    {
        try
        {
            IEnumerable holidays = new List<HolidayModel>();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!isPublished)
            {
                holidays = await _holidayRepository.GetAllHolidayByParticipant(userId);
            }
            else
            {
                holidays = await _holidayRepository.GetAllHolidayPublished();
            }
            var holidayOutDto = _mapper.Map<ICollection<HolidayOutDto>>(holidays);
            
            _logger.LogInformation("Récupération de toutes les vacances du participant {UserId}.", userId);
            return Ok(holidayOutDto);
        }
        catch (LoadDataBaseException ex)
        {
            _logger.LogError("Erreur en base de données lors de la récupération des vacances.");
            return BadRequest(ex.Message);
        }
        catch (RessourceNotFoundException ex)
        {
            _logger.LogError("Erreur en base de données lors de la récupération des vacances.");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    
    /// <summary>
    /// Récupère tous les participants associés à une vacance spécifique en fonction de son identifiant.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la vacance pour laquelle récupérer les participants.</param>
    /// <param name="isParticipated">Booléen qui indique si on veut les participants membres ou non</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec la liste des participants au format ParticipantOutDto si la récupération réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur dans les cas suivants :
    ///   - Aucun participant n'a été trouvé pour la vacance spécifiée.
    ///   - Une exception est levée lors de la récupération des participants.
    /// </returns>
    [HttpGet("{holidayId}/participants")]
    public async Task<IActionResult> GetAllParticipantByHoliday([FromRoute] string holidayId, [FromQuery] bool isParticipated)
    {
        try
        {
            IEnumerable participants = new List<Participant>();
            if (!isParticipated)
            {
                participants = await _participantRepository.GetAllParticipantNotYetInHoliday(new Guid(holidayId));
            }
            else
            {
                participants = await _participantRepository.GetAllParticipantByHoliday(new Guid(holidayId));
            }
            
            var participantsOutDto = _mapper.Map<ICollection<ParticipantOutDto>>(participants);

            _logger.LogInformation("Récupération de tous les participants de la vacances {HolidayId}.", holidayId);
            return Ok(participantsOutDto);
        }
        catch (LoadDataBaseException ex)
        {
            _logger.LogError("Erreur lors de la récupération de tous les participants de la vacances {HolidayId}.", holidayId);
            return BadRequest(ex.Message);
        }
        catch (RessourceNotFoundException ex)
        {
            _logger.LogError("Erreur lors de la récupération de tous les participants de la vacances {HolidayId}.", holidayId);
            return BadRequest(ex.Message);
        }
    }
    
    
    /// <summary>
    /// Récupère une vacance en fonction de son identifiant.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la vacance à récupérer.</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec les informations de la vacance si la récupération réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur dans les cas suivants :
    ///   - Aucune vacance n'a été trouvée pour l'identifiant spécifié.
    ///   - Une exception est levée lors de la récupération de la vacance.
    /// </returns>
    [HttpGet]
    [Route("{holidayId}")]
    public async Task<IActionResult> GetHolidayById([FromRoute] string holidayId)
    {
        try
        {
            var holiday = await _holidayRepository.GetHolidayById(new Guid(holidayId));
            var holidayOutDto = _mapper.Map<HolidayOutDto>(holiday);
            
            _logger.LogInformation("Récupération de la vacances {HolidayUId}.", holidayId);
            return Ok(holidayOutDto);
        }
        catch (LoadDataBaseException ex)
        {
            _logger.LogError("Erreur dans la base de données durant la récupération de la vacances {HolidayId}.", holidayId);
            return BadRequest(ex.Message);
        }
        catch (RessourceNotFoundException ex)
        {
            _logger.LogError("Erreur dans la base de données durant la récupération de la vacances {HolidayId}.", holidayId);
            return BadRequest(ex.Message);
        }
    }

    
    /// <summary>
    /// Crée une nouvelle vacances.
    /// </summary>
    /// <param name="holidayInDto">Les données de la vacances à créer.</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour la demande asynchrone.</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec un message de succès si la création de la vacances et de l'invitation associée réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur si la création de la vacances ou de l'invitation échoue.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateHoliday([FromForm] HolidayInDto holidayInDto, CancellationToken cancellationToken)
    {
        try
        {
            string? pathPicture = null;
            // Traitement de l'image reçue
            if (holidayInDto.UploadedHolidayPicture != null)
            {
                pathPicture = _pictureService.UploadFile(holidayInDto.UploadedHolidayPicture);
            }

            var holidayEntity = _mapper.Map<HolidayModel>(holidayInDto);

            if (!await LocationValidator.IsAddressValidAsync(holidayEntity.Location.GetFormattedAddress()))
            {
                _logger.LogError("L'adresse fournie pour créer la vacances est invalide.");
                return BadRequest("L'adresse fournie n'est pas valide.");
            }

            if (pathPicture != null)
            {
                holidayEntity.HolidayPath = pathPicture;
            }

            if (await SaveHoliday(holidayEntity, cancellationToken))
            {
                _logger.LogInformation("La vacances {HolidayName} a été crée avec succès.", holidayEntity.Name);
                return Ok("Votre vacance a été créée avec succès.");
            }

            _logger.LogError("Une erreur est survenue lors de la création d'une vacances.");
            return BadRequest("Votre vacances n'a pas pu être créée.");

        }
        catch (HolidayStorageException)
        {
            _logger.LogError("Une erreur est survenue lors de l'enregistrement de l'image durant la création d'une vacances.");
            return BadRequest("Un problème est survenu lors de l'enregistrement de l'image");
        }
        catch (LocationException ex)
        {
            _logger.LogError("Une erreur est survenue lors de la création d'une vacances.");
            return BadRequest(ex.Message);
        }
    }
    
    private async Task<bool> SaveHoliday(HolidayModel holidayEntity, CancellationToken cancellationToken)
    {
        if (await _holidayRepository.AddHoliday(holidayEntity, cancellationToken))
        {
            var invitation = new Invitation()
            {
                IsAccepted = true,
                Holiday = holidayEntity,
                ParticipantId = holidayEntity.CreatorId
            };

            if (await _invitationRepository.AddInvitation(invitation, cancellationToken))
            {
                return true;
            }
        }

        return false;
    }
    
    //
    /// <summary>
    /// Met à jour une vacances.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la vacances à mettre à jour</param>
    /// <param name="holidayEditInDto">Les données de la vacance à mettre à jour.</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour la demande asynchrone.</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec un message de succès si la création de la vacance et de l'invitation associée réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur si la création de la vacance ou de l'invitation échoue.
    /// </returns>
    [HttpPut]
    [Route("{holidayId}")]
    public async Task<IActionResult> UpdateHoliday([FromRoute] string holidayId, [FromForm] HolidayEditInDto holidayEditInDto, CancellationToken cancellationToken)
    {
        DeleteOldImage(holidayEditInDto);
        if (!VerifyPathPictureImage(holidayEditInDto, out var pathPicture, out var actionResult)) return actionResult;
    
        var holidayEntity = _mapper.Map<HolidayModel>(holidayEditInDto);
        holidayEntity.Id = new Guid(holidayId);
        
        if (!await LocationValidator.IsAddressValidAsync(holidayEntity.Location.GetFormattedAddress()))
        {
            _logger.LogError("L'adresse fournie lors de la mise à jour de la vacances {HolidayName} n'est pas valide.", holidayEntity.Name);
            return BadRequest("L'adresse fournie n'est pas valide");
        }
        
        // Si une nouvelle image a été définie, on l'assigne
        if (pathPicture != null)
        {
            holidayEntity.HolidayPath = pathPicture;
        }
        // Dans le cas où l'utilisateur n'a pas supprimé ou publié une nouvelle image, on conserve l'ancienne
        if (holidayEditInDto.DeleteImage == false && pathPicture == null)
        {
            holidayEntity.HolidayPath = holidayEditInDto.InitialPath;
        }
    
        if (await _holidayRepository.UpdateHoliday(new Guid(holidayId), holidayEntity, cancellationToken))
        {
            _logger.LogInformation("La vacances {HolidayName} a été mise à jour avec succès.", holidayEntity.Name);
            return Ok("Votre vacances a été mise à jour avec succès.");
        }
        
        _logger.LogError("Une erreur est survenue lors de la mise à jour de la vacances {HolidayName}.", holidayEntity.Name);
        return BadRequest("Votre vacances n'a pas pu être mise à jour.");
    }

    private void DeleteOldImage(HolidayEditInDto holidayEditInDto)
    {
        if (holidayEditInDto.DeleteImage == true)
        {
            try
            {
                _pictureService.deletePicture(holidayEditInDto.InitialPath);
            } catch (HolidayStorageException ex)
            {
                _logger.LogError("Une erreur s'est produite lors de la suppression de l'ancienne image de la vacances.");
            }
        }
    }
    
    private bool VerifyPathPictureImage(HolidayEditInDto holidayEditInDto, out string? pathPicture,
        out IActionResult actionResult)
    {
        pathPicture = null;
        actionResult = null;
        
        // Traitement de l'image reçue
        if (holidayEditInDto.UploadedHolidayPicture != null)
        {
            try
            {
                pathPicture = _pictureService.UploadFile(holidayEditInDto.UploadedHolidayPicture);
            }
            catch (HolidayStorageException)
            {
                _logger.LogError("Une erreur s'est produite lors de l'enregistrement de l'image pour une vacances.");
                actionResult = BadRequest("Un problème est survenu lors de l'enregistrement de l'image.");
                return false;
            }
        }
    
        return true;
    }
    
    /// <summary>
    /// Exporte une vacance au format ICS (Calendrier) en utilisant l'identifiant de la vacance.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la vacance à exporter au format ICS.</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec le fichier ICS représentant la vacance si l'exportation réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur si une exception est levée lors de l'exportation de la vacance en fichier ICS.
    /// </returns>
    [HttpGet("{holidayId}/ics")]
    public async Task<IActionResult> ExportHolidayToIcs([FromRoute] string holidayId)
    {
        try
        {
            var holiday = await _holidayRepository.GetHolidayById(new Guid(holidayId));
            
            string icsContent = CalendarGenerator.ExportEventToIcs(holiday);
            byte[] icsBytes = Encoding.UTF8.GetBytes(icsContent);

            _logger.LogInformation("L'export de la vacances {HolidayId} en fichier ics a été réalisé avec succès.", holidayId);
            return File(icsBytes, "text/calendar", "myHoliday.ics");
        }
        catch (LoadDataBaseException ex)
        {
            _logger.LogError("L'export de la vacances {HolidayId} a provoqué une erreur", holidayId);
            return BadRequest(ex.Message);
        }
        catch (RessourceNotFoundException ex)
        {
            _logger.LogError("L'export de la vacances {HolidayId} a provoqué une erreur", holidayId);
            return BadRequest(ex.Message);
        }
    }
    
    
    /// <summary>
    /// Supprime une vacances sur base son identifiant.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la holiday que l'on souhaite supprimer</param>
    /// <param name="cancellationToken">Un jeton d'annulation permettant d'annuler l'opération de suppression de manière asynchrone.</param>
    /// <returns>Un résultat HTTP indiquant le succès ou l'échec de la suppression de la vacance.</returns>
    [HttpDelete]
    [Route("{holidayId}")]
    public async Task<IActionResult> DeleteHoliday([FromRoute] string holidayId, CancellationToken cancellationToken)
    {

        if (await _holidayRepository.DeleteHoliday(new Guid(holidayId), cancellationToken))
        {
            _logger.LogInformation("La vacances {HolidayId} a été supprimée avec succès.", holidayId);
            return Ok("Vacance supprimée avec succès.");
        }
        
        _logger.LogError("La suppression de la vacances {HolidayId} a provoqué une erreur.", holidayId);
        return BadRequest("Erreur lors de suppression de votre vacance.");
    }
    
    /// <summary>
    /// Quitte une vacances sur base de son identifiant.
    /// </summary>
    /// <param name="holidayId">L'id de la holiday que l'on souhaite quitter</param>
    /// <param name="cancellationToken">Un jeton d'annulation permettant d'annuler l'opération de suppression de manière asynchrone.</param>
    /// <returns>Un résultat HTTP indiquant le succès ou l'échec.</returns>
    [HttpDelete("{holidayId}/leave")]
    public async Task<IActionResult> LeaveHoliday([FromRoute] string holidayId,  CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var idOfTheHoliday = new Guid(holidayId);

            // On supprime l'invitation qui le liait à la holiday
            if (! await _invitationRepository.DeleteInvitationsByParticipant(idOfTheHoliday, userId, cancellationToken))
            {
                _logger.LogError("Une erreur est survenue lorsque l'utilisateur a essayé de quitter la vacances {HolidayID}.", holidayId);
                return BadRequest("Une erreur est survenue lorsque vous avez essayer de quitter l'activité.");
            }
            
            // On supprime toutes les participations qu'il avait à des activiés
            if (!await _participateRepository.DeleteAllParticipateByParticipant(userId, idOfTheHoliday,
                    cancellationToken))
            {
                _logger.LogError("Une erreur est survenue lorsque l'utilisateur a essayé de quitter la vacances {HolidayID}.", holidayId);
                return BadRequest("Une erreur est survenue lorsque vous avez essayer de quitter l'activité.");
            }

            if (!await _invitationRepository.RemainingParticipantsInHoliday(idOfTheHoliday, cancellationToken))
            {
                if (! await _holidayRepository.DeleteHoliday(idOfTheHoliday, cancellationToken))
                {
                    _logger.LogError("Une erreur est survenue lorsque l'utilisateur a essayé de quitter la vacances {HolidayID}.", holidayId);
                    return BadRequest("Une erreur est survenue lorsque vous avez essayer de quitter l'activité.");
                }
            }
            _logger.LogInformation("L'utilisateur a correctement quitté la vacances {HolidayId}.", holidayId);
            return Ok("Vous avez quitter la vacance avec succès.");
        }
        catch (LoadDataBaseException ex)
        {
            _logger.LogError("Une erreur est survenue lorsque l'utilisateur a essayé de quitter la vacances {HolidayID}.", holidayId);
            return BadRequest(ex.Message);
        }
    }
}