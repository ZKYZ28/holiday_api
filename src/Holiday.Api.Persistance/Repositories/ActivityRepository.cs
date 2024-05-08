using System.Data;
using Holiday.Api.Repository.CustomErrors;
using Microsoft.EntityFrameworkCore;
using Activity = Holiday.Api.Repository.Models.Activity;

namespace Holiday.Api.Repository.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly HolidayDbContext _context;
    private readonly IParticipateRepository _participateRepository;
    

    /// <summary>
    /// Initialise une nouvelle instance de la classe ActivityRepository avec le contexte de base de données spécifié.
    /// </summary>
    /// <param name="context">Le contexte de base de données utilisé pour accéder aux activités.</param>
    /// <param name="participateRepository">Interface qui permet l'accès aux données de participants dans la base de données.</param>
    public ActivityRepository(HolidayDbContext context, IParticipateRepository participateRepository) {
        _context = context;
        _participateRepository = participateRepository;
    }
    
    /// <summary>
    /// Ajoute une activité à la base de données de manière asynchrone.
    /// </summary>
    /// <param name="activity">L'activité à ajouter à la base de données.</param>
    /// <param name="cancellationToken">Le jeton d'annulation pour annuler l'opération asynchrone.</param>
    /// <returns>Retourne vrai si l'opération d'ajout s'est déroulée avec succès, sinon retourne faux en cas d'erreur.</returns>
    public async Task<bool> AddActivity(Activity activity, CancellationToken cancellationToken)
    {
        try
        {
            _context.Add(activity);
            await _context.SaveChangesAsync(cancellationToken);
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
        return true;
    }

    /// <summary>
    /// Supprime une activité de la base de données en utilisant son identifiant.
    /// </summary>
    /// <param name="activityId">L'activité à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation pour les opérations asynchrones.</param>
    /// <returns>
    /// - "true" si la suppression de l'activité réussit.
    /// - "false" en cas d'échec de la suppression.
    /// </returns>
    public async Task<bool> DeleteActivity(Guid activityId, CancellationToken cancellationToken)
    {
        try
        {
            var activity = await _context.Activities.FindAsync(activityId);

            if (activity == null)
            {
                return false;
            }
            
            //Permet de supprimer toutes les Participate liées à l'activity
            if (! await _participateRepository.DeleteParticipates(activityId, cancellationToken))
            {
                return false;
            }
            
            _context.Activities.Remove(activity);
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
    /// Supprime les activités liées à des vacances spécifiques en fonction de l'ID de vacances donné.
    /// </summary>
    /// <param name="holidayId">L'ID des vacances pour lesquelles supprimer les activités.</param>
    /// <param name="cancellationToken">Un jeton d'annulation permettant d'annuler l'opération de suppression de manière asynchrone.</param>
    /// <returns>True si la suppression a réussi, sinon False en cas d'erreur.</returns>
    public async Task<bool> DeleteActivities(Guid holidayId, CancellationToken cancellationToken)
    {
        try
        {
            var listActivitiesToDelete = _context.Activities.Where(activity => activity.HolidayId == holidayId).ToList();

            foreach (var activity in listActivitiesToDelete)
            {
                if (! await _participateRepository.DeleteParticipates(activity.Id, cancellationToken))
                {
                    return false;
                }
            }
            
            _context.Activities.RemoveRange(listActivitiesToDelete);
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
    /// Récupère une activité sur base son identifiant
    /// </summary>
    /// <param name="activityId">L'identifiant de l'activité que l'on souhaite récupérer</param>
    /// <returns></returns>
    /// <exception cref="LoadDataBaseException">Une exception qui indique qu'il y a eu une erreur avec la base de données</exception>
    public async Task<Activity> GetActivityById(Guid activityId)
    {
        try
        {
            var activity = await _context.Activities
                .Where(a => a.Id == activityId)
                .Include(h => h.Location)
                .FirstOrDefaultAsync();

            if (activity == null)
            {
                throw new LoadDataBaseException("Erreur lors du chargement de la vacances.");
            }

            return activity;
        }
        catch (Exception ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement de la vacances.");
        }
    }
    

    /// <summary>
    /// Met à jour une activité en base de données sur base de l'activité
    /// </summary>
    /// <param name="updatedActivity">L'activité que l'on souhaite mettre à jour</param>
    /// <param name="cancellationToken">Un jeton d'annulation permettant d'annuler l'opération de mise à jour de manière asynchrone.</param>
    /// <returns></returns>
    public async Task<bool> UpdateActivity(Activity updatedActivity, CancellationToken cancellationToken)
    {
        try
        {
            _context.Entry(updatedActivity).State = EntityState.Modified;
            if(updatedActivity.Location != null) _context.Entry(updatedActivity.Location).State = EntityState.Modified;
            
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return false;
        }
        return true;
    }
}