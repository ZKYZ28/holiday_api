using Holiday.Api.Repository.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace Holiday.Api.Repository.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly HolidayDbContext _context;
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe AuthRepository avec le contexte de base de données spécifié.
    /// </summary>
    /// <param name="context">Le contexte de base de données utilisé pour les opérations d'authentification.</param>
    public AuthRepository(HolidayDbContext context) {
        _context = context;
    }
    
    
    /// <summary>
    /// Recherche un participant dans la base de données par son adresse e-mail de manière asynchrone.
    /// </summary>
    /// <param name="email">L'adresse e-mail du participant à rechercher.</param>
    /// <returns>Une tâche asynchrone qui retourne un objet Participant correspondant à l'adresse e-mail spécifiée.</returns>
    /// <exception cref="LoadDataBaseException">Lancée en cas d'absence d'utilisateur avec l'adresse e-mail spécifiée ou en cas d'erreur lors du chargement des données.</exception>
    public async Task<Models.Participant> GetUserByEmail(string email)
    {
        try
        {
            var participant = await _context.Participants
                .Where(p => p.Email == email)
                .FirstOrDefaultAsync();

            if (participant == null)
            {
                throw new LoadDataBaseException("Aucun utilisateur trouvé avec cet adresse mail.");
            }

            return participant;
        }
        catch (Exception ex)
        {
            throw new LoadDataBaseException("Erreur lors du chargement de l'utilisateur.");
        }
    }
}