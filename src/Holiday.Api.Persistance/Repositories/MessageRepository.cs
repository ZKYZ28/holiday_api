using Holiday.Api.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Holiday.Api.Repository.Repositories;

public class MessageRepository : IMessageRepository
{
    
    private readonly HolidayDbContext _context;
    private const int HistorySize = 100;
       
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe MessageRepository avec le contexte de base de données spécifié.
    /// </summary>
    /// <param name="context">Le contexte de base de données utilisé pour gérer les messages liés aux périodes de vacances.</param>
    public MessageRepository(HolidayDbContext context) {
        _context = context;
    }
    
    /// <summary>
    /// Ajoute un message lié à une période de vacances à la base de données.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur qui envoie le message.</param>
    /// <param name="holidayId">L'identifiant de la période de vacances à laquelle le message est associé.</param>
    /// <param name="content">Le contenu du message.</param>
    /// <returns>Retourne vrai si l'opération d'ajout du message s'est déroulée avec succès, sinon retourne faux en cas d'erreur ou si l'utilisateur ou la période de vacances n'ont pas été trouvés.</returns>
    public async Task<bool> AddMessage(string userId, Guid holidayId, string content)
    {
        try
        {
            var participant = await _context.Participants.SingleOrDefaultAsync(p => p.Id == userId);
            if (participant == null)
            {
                return false;
            }

            var holiday = await _context.Holiday.SingleOrDefaultAsync(h => h.Id == holidayId);
            if (holiday == null)
            {
                return false;
            }

            var message = new Message()
            {
                SendAt = DateTimeOffset.Now,
                Content = content,
                Participant = participant,
                Holiday = holiday
            };


            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// Récupère une liste de messages liés à une période de vacances spécifiée.
    /// </summary>
    /// <param name="holidayId">L'identifiant de la période de vacances pour laquelle on souhaite récupérer les messages.</param>
    /// <returns>Une liste de messages associés à la période de vacances, triée par date d'envoi.</returns>
    public async Task<List<Message>> GetAllMessageByHoliday(Guid holidayId)
    {
        try
        {
            var messages = _context.Messages
                .Include(m => m.Participant)
                .Where(m => m.HolidayId == holidayId)
                .OrderByDescending(m => m.SendAt)
                .Take(HistorySize)
                .OrderBy(m => m.SendAt)
                .ToList();

            return messages;
        }
        catch (Exception e)
        {
            return new List<Message>();
        }
    }


    /// <summary>
    /// Supprimes tous les messages qui sont liés à une vacances.
    /// Utilisé dans le cas où on supprime une vacances
    /// </summary>
    /// <param name="holidayId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> DeleteMessages(Guid holidayId, CancellationToken cancellationToken)
    {
        try
        {
            var listMessagesToDelete = _context.Messages.Where(invitation => invitation.HolidayId == holidayId).ToList();
            _context.Messages.RemoveRange(listMessagesToDelete);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return false;
        }
        
        return true;
    }
}