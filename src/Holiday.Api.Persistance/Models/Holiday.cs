using System.ComponentModel.DataAnnotations.Schema;

namespace Holiday.Api.Repository.Models;

public class Holiday
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }

    public string? HolidayPath { get; set; } = PictureManager.DefaultImageHoliday;
    
    public DateTimeOffset StartDate { get; set; }
    
    public DateTimeOffset EndDate { get; set; }
    
    public bool IsPublish { get; set;}
    
    public string CreatorId { get; set; }
    
    #region EntityFramework
    public Guid LocationId { get; set; } 
    public Location? Location { get; set; }
    
    [NotMapped]
    public ICollection<Participant> Participants { get; set; }
    
    public ICollection<Activity> Activities{ get; set; }
    
    #endregion EntityFramework
    
    
    /// <summary>
    /// Vérifie si la date fournie est valide en comparant la différence entre la date fournie et la date actuelle.
    /// </summary>
    /// <param name="startDate">La date de début à vérifier.</param>
    /// <returns>
    /// True si la date est valide (la différence entre la date fournie et la date actuelle est inférieure à 7 jours), sinon False.
    /// </returns>
    public bool CheckIfDateIsValid(DateTimeOffset startDate)
    {
        return ((startDate - DateTime.Now).TotalDays + 1) <= 3;
    }
}