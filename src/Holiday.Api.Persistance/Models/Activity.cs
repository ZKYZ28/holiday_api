namespace Holiday.Api.Repository.Models;

public class Activity 
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }

    public string ActivityPath { get; set; } = PictureManager.DefaultImageActivity;
    
    public float Price { get; set; }
    
    public DateTimeOffset StartDate { get; set; }
    
    public DateTimeOffset EndDate { get; set; }
    
    #region EntityFramework
    
    /*
     * Cette propriété est automatiquement identifée comme une clé étrangère
     * vers l'entitré Holiday car elle suit la convetion de nommage (de Entity Framework)
     * [Entité]Id pour créer implcitement une relation entre Activity et holiday.
     */
    public Guid HolidayId { get; set; }
    //LIEN VERS LOCATION
    
    public Guid LocationId { get; set; } 
    public Location Location { get; set; }
    
    #endregion EntityFramework

}