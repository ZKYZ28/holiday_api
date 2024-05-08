using System.Data;
using Holiday.Api.Repository.CustomErrors;
using Holiday.Api.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Holiday.Api.Repository.Repositories;

public class ParticipateRepository : IParticipateRepository
{
    private readonly HolidayDbContext _context;
    
    public ParticipateRepository(HolidayDbContext context) {
        _context = context;
    }
    
    
    /// <summary>
    /// Ajoute de manière asynchrone une participation à la base de données en utilisant l'objet Participate fourni.
    /// </summary>
    /// <param name="participate">L'objet Participate à ajouter à la base de données.</param>
    /// <param name="cancellationToken">Un jeton d'annulation permettant d'annuler l'opération d'ajout de manière asynchrone.</param>
    /// <returns>True si l'ajout a réussi, sinon False en cas d'erreur.</returns>
    public async Task<bool> AddParticipate(Participate participate, CancellationToken cancellationToken)
    {
        try
        {
            _context.Participates.Add(participate);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        catch (OperationCanceledException ex)
        {
            return false;
        }
        catch (DBConcurrencyException ex)
        {
            return false;
        }
    }

    /// <summary>
    /// Récupère la liste de tous les participants liés à une activité spécifique en fonction de l'ID de l'activité donné.
    /// </summary>
    /// <param name="activityId">L'identifiant de l'activité pour laquelle récupérer les participants liées.</param>
    /// <param name="cancellationToken">Un jeton d'annulation permettant d'annuler l'opération de récupération de manière asynchrone.</param>
    /// <returns>Une liste de toutes les participations liées à l'activité spécifique.</returns>
    public async Task<IEnumerable<Participant>> GetAllParticipantsByActivity(Guid activityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var participants = _context.Participates
                .Where(p => p.ActivityId == activityId)
                .Select(p => p.Participant)
                .ToList();

            if (participants == null)
            {
                throw new RessourceNotFoundException("Les participants n'ont pas pu être trouvés.");
            }
            
            return participants;
        }
        catch (DbUpdateException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants d'une activité.");
        }
        catch (OperationCanceledException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants d'une activité.");
        }
        catch (DBConcurrencyException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des participants d'une activité.");
        }
    }
    
    /// <summary>
    /// Supprime un objet Participate de la base de données .
    /// </summary>
    /// <param name="activityId">L'identifiant du l'activité à laquelle on souhaite supprimer la participation</param>
    /// <param name="userId">L'identifiant du participant auquel on souhaite supprimer la participation</param>
    /// <param name="cancellationToken">Le jeton d'annulation.</param>
    /// <returns>True si la suppression réussit, False en cas d'erreur.</returns>
    public async Task<bool> RemoveParticipate(Guid activityId, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var participate = await _context.Participates
                .FirstOrDefaultAsync(p => p.ActivityId == activityId && p.ParticipantId == userId, cancellationToken);

            if (participate == null)
            {
                return false;
            }
            
            _context.Participates.Remove(participate);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        catch (OperationCanceledException ex)
        {
            return false;
        }
        catch (DBConcurrencyException ex)
        {
            return false;
        }
    }

    /// <summary>
    /// Supprime tous les Participates liés à une activité spécifiée.
    /// </summary>
    /// <param name="activityId">L'identifiant de l'activité dont les participations doivent être supprimées.</param>
    /// <param name="cancellationToken">Le jeton d'annulation.</param>
    /// <returns>True si la suppression réussit, False en cas d'erreur.</returns>
    public async Task<bool> DeleteParticipates(Guid activityId, CancellationToken cancellationToken)
    {
        try
        {
            var participatesToDelete = _context.Participates
                .Where(p => p.ActivityId == activityId)
                .ToList();
            
            _context.Participates.RemoveRange(participatesToDelete);
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        catch (OperationCanceledException ex)
        {
            return false;
        }
        catch (DBConcurrencyException ex)
        {
            return false;
        }
    }

    /// <summary>
    /// Supprime tous les Participates liés à un participant et à une vacance spécifique.
    /// </summary>
    /// <param name="participantId">L'identifiant du participant dont les participations doivent être supprimées.</param>
    /// <param name="holidayId">L'identifiant de la vacance</param>
    /// <param name="cancellationToken">Le jeton d'annulation.</param>
    /// <returns>True si la suppression réussit, False en cas d'erreur.</returns>
    public async Task<bool> DeleteAllParticipateByParticipant(string participantId,
        Guid holidayId, CancellationToken cancellationToken)
    {
        try
        {
            var participationsToLeave =  _context.Participates
                .Where(participation => participation.ParticipantId == participantId)
                .Where(participation => _context.Activities
                    .Any(activity => activity.Id == participation.ActivityId && activity.HolidayId == holidayId))
                .ToList();
            
            _context.Participates.RemoveRange(participationsToLeave);
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        catch (OperationCanceledException ex)
        {
            return false;
        }
        catch (DBConcurrencyException ex)
        {
            return false;
        }
    }
}