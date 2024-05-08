using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Holiday.Api.Repository.Models;

public class Participant : IdentityUser
{
    public string LastName { get; set; }
    
    public string FirstName { get; set; }
    
    public static Participant CreateExternalLoginParticipant(string email, string? familyName, string firstName)
    {
        return new Participant
        {
            Email = email,
            UserName = email,
            LastName = familyName ?? "",
            // Un prénom doit toujours être défini chez Google
            FirstName = firstName,
        };
    }
    
}