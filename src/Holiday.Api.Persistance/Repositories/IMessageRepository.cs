using Holiday.Api.Repository.Models;

namespace Holiday.Api.Repository.Repositories;

public interface IMessageRepository
{
    Task<bool> AddMessage(string userID, Guid holidayId, string message);

    Task<List<Message>> GetAllMessageByHoliday(Guid holidayId);

    Task<bool> DeleteMessages(Guid holidayId,CancellationToken cancellationToken);
}