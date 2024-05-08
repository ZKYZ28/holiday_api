using System.ComponentModel.DataAnnotations;

namespace Holiday.Api.Repository.Models;

public class Message
{
    public Guid Id { get; set; }

    public DateTimeOffset SendAt { get; set; }

    public string Content { get; set; }

    #region ForeignKey

    public Holiday Holiday { get; set; }
    public Guid HolidayId { get; set; }
    
    public Participant Participant { get; set; }
    public string ParticipantId { get; set; }
    
    #endregion
    
}