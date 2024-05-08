using Holiday.Api.Repository.Models;

namespace Holiday.Api.Repository.Repositories;

public interface IHolidayRepository
{
    Task<IEnumerable<Models.Holiday>>? GetAllHolidayByParticipant(string participantId);

    Task<IEnumerable<Models.Holiday>> GetAllHolidayPublished();

    Task<Models.Holiday> GetHolidayById(Guid holidayId);
    
    Task<bool> AddHoliday(Models.Holiday holiday, CancellationToken cancellationToken = default);
    Task<bool> UpdateHoliday(Guid holidayId, Models.Holiday holiday, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteHoliday(Guid holiday, CancellationToken cancellationToken = default);

    Task<List<Statistics>> GetCountHolidaysForDate(DateTimeOffset date);
    
}