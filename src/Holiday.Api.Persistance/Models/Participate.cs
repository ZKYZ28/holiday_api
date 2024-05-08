namespace Holiday.Api.Repository.Models;

public class Participate
{
    public Guid Id { get; set; }
    
    #region EntityFramework
    public Activity Activity { get; set; }
    public Guid ActivityId { get; set; }
    
    public Participant Participant { get; set; }
    public string ParticipantId { get; set; }
    
    #endregion EntityFramework
}