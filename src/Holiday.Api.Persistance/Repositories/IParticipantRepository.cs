using Holiday.Api.Repository.Models;

namespace Holiday.Api.Repository.Repositories;

public interface IParticipantRepository
{ 
    Task<IEnumerable<Models.Participant>> GetAllParticipantNotYetInHoliday(Guid holidayId);
    
    Task<IEnumerable<Models.Participant>> GetAllParticipantByHoliday(Guid holidayId);

    int GetParticipantCount();

    Task<IEnumerable<Models.Participant>> GetParticipantsNotYetInActivity(Guid activityId,
        CancellationToken cancellationToken);
}