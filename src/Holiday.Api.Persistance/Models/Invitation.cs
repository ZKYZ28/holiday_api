using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Holiday.Api.Repository.Models;

public class Invitation
{
    public Guid Id { get; set; }
    public bool IsAccepted { get; set; } = false;
    
    
     #region EntityFramework
     public Holiday Holiday { get; set; }
    public Guid HolidayId { get; set; }
    
    public Participant Participant { get; set; }
    public string ParticipantId { get; set; }
    
    #endregion EntityFramework
}