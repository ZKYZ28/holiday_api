using Holiday.Api.Repository.Models;

namespace Holiday.Api.Repository.Repositories;

public interface IParticipateRepository
{
    Task<bool> RemoveParticipate(Guid activityId, string userId, CancellationToken cancellationToken);
    
    Task<bool> AddParticipate(Participate participate, CancellationToken cancellationToken);
    
    Task<IEnumerable<Participant>> GetAllParticipantsByActivity(Guid activityId, CancellationToken cancellationToken);
    
    Task<bool> DeleteParticipates(Guid activityId, CancellationToken cancellationToken);
    
    Task<bool> DeleteAllParticipateByParticipant(string participantId, Guid holidayId, CancellationToken cancellationToken);
}