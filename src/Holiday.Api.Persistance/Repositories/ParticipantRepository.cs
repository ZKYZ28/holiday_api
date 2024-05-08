using System.Data;
using Holiday.Api.Repository.CustomErrors;
using Holiday.Api.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Holiday.Api.Repository.Repositories;

public class ParticipantRepository : IParticipantRepository
{
    private readonly HolidayDbContext _context;
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe ParticipantRepository avec le contexte de base de données spécifié.
    /// </summary>
    /// <param name="context">Le contexte de base de données utilisé pour gérer les participants aux périodes de vacances.</param>
    public ParticipantRepository(HolidayDbContext context) {
        _context = context;
    }
    
    /// <summary>
    /// Récupère la liste de tous les participants qui ne sont pas encore dans une vacance donnée.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la période de vacances pour laquelle on souhaite récupérer les participants.</param>
    /// <returns>Une collection de participants non encore acceptés pour la période de vacances spécifiée.</returns>
    /// <exception cref="LoadDataBaseException">Lancée en cas d'erreur lors du chargement des participants depuis la base de données.</exception>
    public async Task<IEnumerable<Participant>> GetAllParticipantNotYetInHoliday(Guid holidayId)
    {
        try
        {
            var participants = await _context.Participants
                .Where(participant => !_context.Invitations.Any(invitation => invitation.ParticipantId == participant.Id && invitation.HolidayId == holidayId))
                .ToListAsync();
            
            return participants;

        }
        catch (DbUpdateException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants.");
        }
        catch (OperationCanceledException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants.");
        }
        catch (DBConcurrencyException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants.");
        }
    }
    
    /// <summary>
    /// Récupère le nombre total de participants enregistrés dans la base de données.
    /// </summary>
    /// <returns>Le nombre total de participants enregistrés.</returns>
    /// <exception cref="LoadDataBaseException">Lancée en cas d'erreur lors du chargement du nombre de participants depuis la base de données.</exception>
    public int GetParticipantCount()
    {
        try
        {
            return _context.Participants.Count();
        }
        catch (ArgumentNullException e)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants");
        }
        catch (OverflowException e)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants");
        }
        catch (DBConcurrencyException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants");
        }
    }
    

    /// <summary>
    /// Récupère la liste de tous les participants ayant acceptés une invitation pour une période de vacances spécifiée.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la période de vacances pour laquelle on souhaite récupérer les participants acceptés.</param>
    /// <returns>Une collection de participants ayant accepté une invitation pour la période de vacances spécifiée.</returns>
    /// <exception cref="LoadDataBaseException">Lancée en cas d'erreur lors du chargement des participants depuis la base de données.</exception>
    public async Task<IEnumerable<Participant>> GetAllParticipantByHoliday(Guid holidayId)
    {
        try
        {
            var participants = _context.Participants
                .Join(
                    _context.Invitations,
                    participant => participant.Id,
                    invitation => invitation.ParticipantId,
                    (participant, invitation) => new
                    {
                        Participant = participant,
                        Invitation = invitation
                    })
                .Where(joinResult => joinResult.Invitation.HolidayId == holidayId && joinResult.Invitation.IsAccepted == true)
                .Select(joinResult => joinResult.Participant)
                .ToList();
             

            if (participants == null)
            {
                throw new RessourceNotFoundException("Participants non trouvés.");
            }

            return participants;

        }
        catch (ArgumentNullException e)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants");
        }
        catch (OverflowException e)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants");
        }
        catch (DBConcurrencyException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants");
        }
    }


    /// <summary>
    /// Récupère la liste des participants qui ne sont pas encore inscrits à une activité spécifique en fonction de l'identifiant de l'activité donné.
    /// </summary>
    /// <param name="activityId">L'identifiant de l'activité pour laquelle rechercher les participants non inscrits.</param>
    /// <param name="cancellationToken">Un jeton d'annulation permettant d'annuler l'opération de récupération de manière asynchrone.</param>
    /// <returns>Une liste de participants non encore inscrits à l'activité spécifique.</returns>
    public async Task<IEnumerable<Participant>> GetParticipantsNotYetInActivity(Guid activityId, CancellationToken cancellationToken)
    {
        var participants = new List<Participant>();
        try
        {
            var activity = _context.Activities.Find(activityId);

            if (activity != null)
            {
                participants = await _context.Participants
                    .Join(
                        _context.Invitations,
                        participant => participant.Id,
                        invitation => invitation.ParticipantId,
                        (participant, invitation) => new
                        {
                            Participant = participant,
                            Invitation = invitation
                        })
                    .Where(joinResult => joinResult.Invitation.HolidayId == activity.HolidayId && joinResult.Invitation.IsAccepted)
                    .Where(joinResult => !_context.Participates
                        .Any(pa => pa.ParticipantId == joinResult.Participant.Id && pa.ActivityId == activityId))
                    .Select(joinResult => joinResult.Participant)
                    .ToListAsync(cancellationToken);
            }

            if (participants == null)
            {
                throw new RessourceNotFoundException("Nous n'avons pas pu trouver la liste des participants.");
            }
            
            return participants;
        }
        catch (ArgumentNullException e)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants.");
        }
        catch (OverflowException e)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants.");
        }
        catch (DBConcurrencyException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants.");
        }
    }


}