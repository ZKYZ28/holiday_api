using Holiday.Api.Repository.Models;

namespace Holiday.Api.Repository.Repositories;

public interface IActivityRepository
{
    Task<bool> AddActivity(Models.Activity activity, CancellationToken cancellationToken);
    
    Task<bool> DeleteActivity(Guid activityId, CancellationToken cancellationToken);
    
    Task<bool> DeleteActivities(Guid holidayId, CancellationToken cancellationToken);
    Task<Activity> GetActivityById(Guid activityId);
    Task<bool> UpdateActivity(Activity activityEntity, CancellationToken cancellationToken);
}