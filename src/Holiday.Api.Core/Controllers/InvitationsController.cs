using System.Collections;
using System.Security.Claims;
using AutoMapper;
using DefaultNamespace;
using Holiday.Api.Contract.Dto;
using Holiday.Api.Repository.CustomErrors;
using Holiday.Api.Repository.Models;
using Holiday.Api.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Holiday.Api.Core.Controllers;

[ApiController]
[Authorize]
[Route("/v1/invitations")]
public class InvitationsController : Controller
{
    private readonly IMapper _mapper;
    private readonly ILogger<InvitationsController> _logger;
    private readonly IInvitationRepository _invitationRepository;

    
    /// <summary>
    /// Initialise une nouvelle instance de la classe InvitationController.
    /// </summary>
    /// <param name="mapper">L'objet de mappage utilisé pour mapper des objets entre différents modèles.</param>
    /// <param name="logger">Le journal (logger) utilisé pour l'enregistrement des messages de journalisation.</param>
    /// <param name="invitationRepository">Interface qui permet l'accès aux données des invitations.</param>
    /// <param name="participantRepository">Interface qui permet l'accès aux données des participants.</param>
    public InvitationsController([FromServices] IMapper mapper, ILogger<InvitationsController> logger, [FromServices] IInvitationRepository invitationRepository, [FromServices] IParticipantRepository participantRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _invitationRepository = invitationRepository;
    }
    
    
    /// <summary>
    /// Crée de nouvelles invitations à une vacances.
    /// </summary>
    /// <param name="invitationsInDto">Les données des invitations à créer.</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour la demande asynchrone.</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec un message de succès si la création des invitations réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur si la création des invitations échoue.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateInvitationsAsync([FromBody] InvitationInDto[] invitationsInDto, CancellationToken cancellationToken)
    {
        foreach (var invitationInDto in invitationsInDto)
        {
            var invitation = _mapper.Map<Invitation>(invitationInDto);

            if (!await _invitationRepository.AddInvitation(invitation, cancellationToken))
            {
                _logger.LogError("Une erreur en base de données est survenue lors de la création d'une invitation.");
                return BadRequest("Vos invitations n'ont pas pu être envoyées.");
            }
        }
        
        _logger.LogError("Les invitations ont été créées avec succès.");
        return Ok( "Invitations envoyées avec succès.");
    }
    
    
    /// <summary>
    /// Récupère toutes les invitations associées à l'utilisateur connecté.
    /// </summary>
    /// <returns>
    /// - StatusCode 200 (OK) avec la liste des invitations au format InvitationOutDto si la récupération réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur dans les cas suivants :
    ///   - Aucune invitation n'a été trouvée pour le participant.
    ///   - Une exception est levée lors de la récupération des invitations.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAllInvitationsByParticipant()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var invitations = await _invitationRepository.GetInvitationByParticipant(userId);
            var invitationOutDtoList = _mapper.Map<ICollection<InvitationOutDto>>(invitations);
            
            _logger.LogInformation("Récupération de toutes les invitations du participant {UserId}.", userId);
            return Ok(invitationOutDtoList);
        }
        catch (LoadDataBaseException ex)
        {
            _logger.LogError("Une erreur est survenue en base de données durant la récupération de toutes les invitations.");
            return BadRequest(ex.Message);
        }
        catch (RessourceNotFoundException ex)
        {
            _logger.LogError("Une erreur est survenue durant la récupération de toutes les invitations.");
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Accepte une invitation sur base de son identifiant.
    /// </summary>
    /// <param name="invitationId">L'identifiant de l'invitation que l'on souhaite accepter.</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour la demande asynchrone.</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec un message de succès si l'invitation est acceptée avec succès.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur dans les cas suivants :
    ///   - L'invitation ou la vacance associée n'a pas pu être trouvée.
    ///   - Une exception est levée lors de l'acceptation de l'invitation.
    /// </returns>
    [HttpPut]
    [Route("{invitationId}")]
    public async Task<IActionResult> AcceptInvitation([FromRoute] string invitationId, CancellationToken cancellationToken)
    {
        
        if (! await _invitationRepository.AcceptInvitation(new Guid(invitationId), cancellationToken))
        {
            _logger.LogError("Une erreur est survenue lors de l'acceptation de l'invitation {InvitationId}.", invitationId);
            return BadRequest("Erreur lors de l'acceptation de l'invitation.");
        } 
        
        _logger.LogInformation("Acceptation de l'invitation {InvitationId} réalisée avec succès.", invitationId);
        return Ok("Invitation acceptée.");
    }
    
    
    /// <summary>
    /// Supprime une invitation sur base de son identifiant.
    /// </summary>
    /// <param name="invitationId">L'identifiant de l'invitation que l'on souhaite refuser</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour la demande asynchrone.</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec un message de succès si l'invitation est refusée avec succès.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur dans les cas suivants :
    ///   - L'invitation ou la vacance associée n'a pas pu être trouvée.
    ///   - Une exception est levée lors du refus de l'invitation.
    /// </returns>
    [HttpDelete]
    [Route("{invitationId}")]
    public async Task<IActionResult> RefuseInvitation([FromRoute] string invitationId, CancellationToken cancellationToken)
    {
        if (!await _invitationRepository.RefuseInvitation(new Guid(invitationId), cancellationToken))
        {
            _logger.LogError("Une erreur est survenue lors de la suppression de l'invitation {InvitationId}.", invitationId);
             return BadRequest("Erreur lors du refus de l'invitation.");
        }
        
        _logger.LogInformation("Suppression de l'invitation {InvitationId} réalisée avec succès.", invitationId);
        return Ok( "Invitation refusée.");
    }
}