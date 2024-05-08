
using System.Data;
using Holiday.Api.Repository.CustomErrors;
using Holiday.Api.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Holiday.Api.Repository.Repositories;

public class HolidayRepository : IHolidayRepository
{
    
    private readonly HolidayDbContext _context;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IMessageRepository _messageRepository;
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe HolidayRepository avec le contexte de base de données spécifié.
    /// </summary>
    /// <param name="context">Le contexte de base de données utilisé pour accéder aux informations sur les vacances.</param>
    /// <param name="invitationRepository">Interface qui permet l'accès aux données des invitations.</param>
    /// <param name="activityRepository">Interface qui permet l'accès aux données d'activités dans la base de données.</param>
    /// <param name="messageRepository">Interface qui permet l'accès aux données des messages dans la base de données.</param>
    public HolidayRepository(HolidayDbContext context, IInvitationRepository invitationRepository,  IActivityRepository activityRepository, IMessageRepository messageRepository) {
        _context = context;
        _invitationRepository = invitationRepository;
        _activityRepository = activityRepository;
        _messageRepository = messageRepository;
    }
    
    
    
    /// <summary>
    /// Met à jour une période de vacances dans la base de données.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la vacances que l'on souhaite mettre à jour</param>
    /// <param name="updatedHoliday">La vacances à mettre à jour dans la base de données.</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour annuler l'opération asynchrone.</param>
    /// <returns>Retourne vrai si l'opération d'ajout s'est déroulée avec succès, sinon retourne faux en cas d'erreur.</returns>
    public async Task<bool> UpdateHoliday(Guid holidayId, Models.Holiday updatedHoliday, CancellationToken cancellationToken)
    {
        try
        {
            _context.Entry(updatedHoliday).State = EntityState.Modified;
            if(updatedHoliday.Location != null) _context.Entry(updatedHoliday.Location).State = EntityState.Modified;
            
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Récupère toutes les périodes de vacances associées à un participant spécifique sur base de son identifiant.
    /// </summary>
    /// <param name="participantId">L'identifiant du participant pour lequel on souhaite récupérer les vacances.</param>
    /// <returns>Une collection de périodes de vacances liées au participant spécifié.</returns>
    /// <exception cref="LoadDataBaseException">Lancée en cas d'erreur lors du chargement des données depuis la base de données.</exception>
    public async Task<IEnumerable<Models.Holiday>> GetAllHolidayByParticipant(string participantId)
    {
        try
        {
            var holidays =  await _context.Holiday
                .Include(h => h.Location)
                .Join(
                    _context.Invitations.Where(invitation => invitation.IsAccepted && invitation.ParticipantId == participantId),
                    holiday => holiday.Id,
                    invitation => invitation.HolidayId,
                    (holiday, invitation) => holiday
                )
                .ToListAsync();
            
            if (holidays == null)
            {
                throw new RessourceNotFoundException("Les vacances du participant n'ont pas pu être trouvées.");
            }

            return holidays;
        }
        catch (DbUpdateException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement de toutes les vacances.", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement de toutes les vacances.", ex);
        }
        catch (DBConcurrencyException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement de toutes les vacances.", ex);
        }
    }
    
    /// <summary>
    /// Récupère toutes les périodes de vacances qui sont publiées.
    /// </summary>
    /// <returns>Une collection de périodes de vacances publiées avec leurs emplacements.</returns>
    /// <exception cref="LoadDataBaseException">Lancée en cas d'erreur lors du chargement des données depuis la base de données.</exception>
    public async Task<IEnumerable<Models.Holiday>> GetAllHolidayPublished()
    {
        try
        {
            var holidaysPublished =  await _context.Holiday
                .Where(holiday => holiday.IsPublish)
                .Include(h => h.Location)
                .ToListAsync();
            
            if (holidaysPublished == null)
            {
                throw new RessourceNotFoundException("Les vacances publiées n'ont pas pu être trouvées.");
            }

            return holidaysPublished;
        }
        catch (ArgumentNullException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement de toutes les vacances publiées.", ex);
        }
        catch (OperationCanceledException  ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement de toutes les vacances publiées.", ex);
        }
        catch (DBConcurrencyException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement de toutes les vacances publiées.", ex);
        }
    }
    

    
    /// <summary>
    /// Récupère une période de vacances par son identifiant.
    /// </summary>
    /// <param name="holidayId">L'identifiant unique de la période de vacances à récupérer.</param>
    /// <returns>Un objet Holiday représentant la période de vacances avec ses détails.</returns>
    /// <exception cref="LoadDataBaseException">Lancée en cas d'erreur lors du chargement des données depuis la base de données.</exception>
    public async Task<Models.Holiday> GetHolidayById(Guid holidayId)
    {
        try
        {
            var holiday = await _context.Holiday
                .Where(h => h.Id == holidayId)
                .Include(h => h.Location)
                .Include(h => h.Activities)
                .ThenInclude(a => a.Location)
                .FirstOrDefaultAsync();
            
            var acceptedParticipantIds = await _context.Invitations
                .Where(invitation => invitation.HolidayId == holidayId && invitation.IsAccepted)
                .Select(invitation => invitation.ParticipantId)
                .ToListAsync();

            holiday.Participants = await _context.Participants
                .Where(participant => acceptedParticipantIds.Contains(participant.Id))
                .ToListAsync();

            if (holiday == null)
            {
                throw new RessourceNotFoundException("La vacances n'a pas pu être trouvée.");
            }
            
            return holiday;
        }
        catch (DbUpdateException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement de la vacances.", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement de la vacances.", ex);
        }
        catch (DBConcurrencyException ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement de la vacances.", ex);
        }
    }
    

    /// <summary>
    /// Ajoute une période de vacances à la base de données.
    /// </summary>
    /// <param name="holiday">La période de vacances à ajouter à la base de données.</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour annuler l'opération asynchrone.</param>
    /// <returns>Retourne vrai si l'opération d'ajout s'est déroulée avec succès, sinon retourne faux en cas d'erreur.</returns>
    public async Task<bool> AddHoliday(Models.Holiday holiday, CancellationToken cancellationToken)
    {
        try
        {
            _context.Add(holiday);
            await _context.SaveChangesAsync(cancellationToken);
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
        
        return true;
    }
    
    
    
    /// <summary>
    /// Récupère le nombre de périodes de vacances qui incluent une date donnée.
    /// </summary>
    /// <param name="date">La date pour laquelle on souhaite obtenir le nombre de vacances.</param>
    /// <returns>Le nombre de périodes de vacances qui incluent la date spécifiée.</returns>
    /// <exception cref="LoadDataBaseException">Lancée en cas d'erreur lors de la récupération du nombre de vacances pour la date donnée.</exception>
    public async Task<List<Statistics>> GetCountHolidaysForDate(DateTimeOffset date)
    {
        try
        {
            var result = await _context.Invitations
                .Where(invitation => invitation.IsAccepted &&
                                     invitation.Holiday.StartDate <= date &&
                                     invitation.Holiday.EndDate >= date)
                .GroupBy(invitation => invitation.Holiday.Location.Country)
                .Select(group => new Statistics
                {
                    Country = group.Key,
                    ParticipantsByCountry = group.Count()
                })
                .ToListAsync();

            return result;
        } 
        catch (DbUpdateException ex)
        {
            throw new LoadDataBaseException("Erreur lors de la récupération du nombre de vacances pour une date donnée.", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new LoadDataBaseException("Erreur lors de la récupération du nombre de vacances pour une date donnée.", ex);
        }
        catch (DBConcurrencyException ex)
        {
            throw new LoadDataBaseException("Erreur lors de la récupération du nombre de vacances pour une date donnée.", ex);
        }
    }
    
    
    /// <summary>
    /// Supprime une vacance ainsi que toutes ses activités, invitations et messages associés de manière asynchrone.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la vacances que l'on souhaite supprimer.</param>
    /// <param name="cancellationToken">Un jeton d'annulation permettant d'annuler l'opération de suppression de manière asynchrone.</param>
    /// <returns>True si la suppression a réussi, sinon False en cas d'erreur.</returns>
    public async Task<bool> DeleteHoliday(Guid holidayId, CancellationToken cancellationToken = default)
    {   
        // Permet de revenir en arrière si une opération se passe mal
        using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                var holiday = await _context.Holiday.FindAsync(holidayId);

                if (holiday == null)
                {
                    return false;
                }
                
                if (!await _activityRepository.DeleteActivities(holidayId, cancellationToken))
                {
                    transaction.Rollback();
                    return false;
                }

                if (!await _invitationRepository.DeleteInvitations(holidayId, cancellationToken))
                {
                    transaction.Rollback(); 
                    return false;
                }

                if (!await _messageRepository.DeleteMessages(holidayId, cancellationToken))
                {
                    transaction.Rollback(); 
                    return false;
                }

                
                _context.Holiday.Remove(holiday);
                await _context.SaveChangesAsync(cancellationToken);

                // Valider la transaction
                transaction.Commit(); 

                return true;
            }
            catch (DbUpdateException ex)
            {
                // Annuler la transaction en cas d'erreur
                transaction.Rollback(); 
                return false;
            }
            catch (InvalidOperationException ex)
            {
                transaction.Rollback(); 
                return false;
            }
            catch (DBConcurrencyException ex)
            {
                transaction.Rollback(); 
                return false;
            }
        }
    }
    
}