using System.Data;
using Holiday.Api.Repository.CustomErrors;
using Holiday.Api.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Holiday.Api.Repository.Repositories;

public class InvitationRepository : IInvitationRepository
{
    private readonly HolidayDbContext _context;
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe InvitationRepository avec le contexte de base de données spécifié.
    /// </summary>
    /// <param name="context">Le contexte de base de données utilisé pour gérer les invitations liées aux périodes de vacances.</param>
    public InvitationRepository(HolidayDbContext context) {
        _context = context;
    }
    
    /// <summary>
    /// Ajoute une invitation à une période de vacances à la base de données.
    /// </summary>
    /// <param name="invitation">L'invitation à ajouter à la base de données.</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour annuler l'opération asynchrone.</param>
    /// <returns>Retourne vrai si l'opération d'ajout de l'invitation s'est déroulée avec succès, sinon retourne faux en cas d'erreur.</returns>
    public async Task<bool> AddInvitation(Invitation invitation, CancellationToken cancellationToken)
    {
        try
        {
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        catch (InvalidOperationException ex)
        {
            return false;
        }
        catch (DBConcurrencyException ex)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Récupère toutes les invitations non acceptées d'un participant spécifique sur base de son identifiant.
    /// </summary>
    /// <param name="participantId">L'identifiant du participant pour lequel on souhaite récupérer les invitations non acceptées.</param>
    /// <returns>Une collection d'invitations non acceptées par le participant spécifié.</returns>
    /// <exception cref="Exception">Lancée en cas d'erreur lors du chargement des invitations non acceptées.</exception>
    public async Task<IEnumerable<Invitation>> GetInvitationByParticipant(string participantId)
    {
        try
        {
            var unacceptedInvitations = await _context.Invitations
                .Where(invitation => !invitation.IsAccepted && invitation.ParticipantId == participantId)
                .Include(invitation => invitation.Holiday)
                .ThenInclude(holiday => holiday.Location) 
                .Include(invitation => invitation.Participant)
                .ToListAsync();
            
            if (unacceptedInvitations == null)
            {
                throw new RessourceNotFoundException("Les invitations n'ont pas pu être trouvées.");
            }

            return unacceptedInvitations;
        }
        catch (DbUpdateException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des invitations du participant.", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des invitations du participant.", ex);
        }
        catch (DBConcurrencyException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement des invitations du participant.", ex);
        }
    }
    
    /// <summary>
    /// Accepte une invitation de participation à une période de vacances dans la base de données.
    /// </summary>
    /// <param name="invitationId">L'identifiant de l'invitation à accepter.</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour annuler l'opération asynchrone.</param>
    /// <returns>Retourne vrai si l'opération d'acceptation de l'invitation s'est déroulée avec succès, sinon retourne faux en cas d'erreur ou si l'invitation est nulle.</returns>
    public async Task<bool> AcceptInvitation(Guid invitationId, CancellationToken cancellationToken)
    {
        try
        {
            var invitation = _context.Invitations.FirstOrDefault(i => i.Id == invitationId);

            if (invitation != null)
            {
                invitation.IsAccepted = true;

                await _context.SaveChangesAsync(cancellationToken);

                return true;
            }
            else
            {
                return false;
            }
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        catch (InvalidOperationException ex)
        {
            return false;
        }
        catch (DBConcurrencyException ex)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Refuse une invitation de participation à une période de vacances en la supprimant de la base de données.
    /// </summary>
    /// <param name="invitationId">L'identifiant de l'invitation à refuser.</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour annuler l'opération asynchrone.</param>
    /// <returns>Retourne vrai si l'opération de refus de l'invitation s'est déroulée avec succès, sinon retourne faux en cas d'erreur ou si l'invitation est nulle.</returns>
    public async Task<bool> RefuseInvitation(Guid invitationId, CancellationToken cancellationToken)
    {
        try
        {
            var invitation = await _context.Invitations.FindAsync(invitationId);

            if (invitation == null)
            {
                return false;
            }
            
            _context.Remove(invitation);
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        catch (InvalidOperationException ex)
        {
            return false;
        }
        catch (DBConcurrencyException ex)
        {
            return false;
        }
    }
    
    
    /// <summary>
    /// Supprime toutes les invitations liées à une vacances spécifique en fonction de l'identifiant de celle-ci.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la vacance pour laquelle on souhaite supprimer les invitations.</param>
    /// <param name="cancellationToken">Un jeton d'annulation permettant d'annuler l'opération de suppression de manière asynchrone.</param>
    /// <returns>True si la suppression a réussi, sinon False en cas d'erreur.</returns>
    public async Task<bool> DeleteInvitations(Guid holidayId, CancellationToken cancellationToken)
    {
        try
        {
            var listInvitationsToDelete = _context.Invitations.Where(invitation => invitation.HolidayId == holidayId).ToList();
            _context.Invitations.RemoveRange(listInvitationsToDelete);
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        catch (InvalidOperationException ex)
        {
            return false;
        }
        catch (DBConcurrencyException ex)
        {
            return false;
        }
    }
    
    
    /// <summary>
    /// Supprime une invitation spécifique en fonction de l'identification de vacances et de l'identifiant du participant donnés.
    /// Utilisez dans le cas où un participant quitte un groupe.
    /// </summary>
    /// <param name="holidayId">L'identifiant des vacances liées à l'invitation à supprimer.</param>
    /// <param name="participantId">L'identifiant du participant lié à l'invitation à supprimer.</param>
    /// <param name="cancellationToken">Un jeton d'annulation permettant d'annuler l'opération de suppression de manière asynchrone.</param>
    /// <returns>True si la suppression a réussi, sinon False en cas d'erreur.</returns>
    public async Task<bool> DeleteInvitationsByParticipant(Guid holidayId, string participantId, CancellationToken cancellationToken)
    {
        try
        {
            var invitation = await _context.Invitations.Where(invitation => invitation.HolidayId == holidayId && invitation.ParticipantId == participantId).FirstAsync();
            _context.Invitations.Remove(invitation);
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        catch (InvalidOperationException ex)
        {
            return false;
        }
        catch (DBConcurrencyException ex)
        {
            return false;
        }
    }


    /// <summary>
    /// Compte de manière asynchrone le nombre d'invitations acceptées associées à des vacances spécifiques en fonction de l'activities de la vacances donné.
    /// Utilisé lorsqu'un participant quitte une vacance => on vérifie si il reste des participants dans la vacance.
    /// </summary>
    /// <param name="holidayId">L'ID des vacances pour lesquelles compter les invitations acceptées.</param>
    /// <param name="cancellationToken">Un jeton d'annulation permettant d'annuler l'opération de comptage de manière asynchrone.</param>
    /// <returns>Le nombre d'invitations acceptées associées aux vacances spécifiques.</returns>
    public async Task<bool> RemainingParticipantsInHoliday(Guid holidayId, CancellationToken cancellationToken)
    {
        try
        {
            var countInvitation = await _context.Invitations
                .Where(invitation => invitation.HolidayId == holidayId && invitation.IsAccepted)
                .CountAsync(cancellationToken);

            if (countInvitation == 0)
            {
                return false;
            }
            
            return true;
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        catch (InvalidOperationException ex)
        {
            return false;
        }
        catch (DBConcurrencyException ex)
        {
            return false;
        }
    }
}