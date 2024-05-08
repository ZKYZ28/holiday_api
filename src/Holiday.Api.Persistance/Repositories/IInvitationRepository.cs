using Holiday.Api.Repository.Models;

namespace Holiday.Api.Repository.Repositories;

public interface IInvitationRepository
{
    Task<bool> AddInvitation(Invitation invitation, CancellationToken cancellationToken);

    Task<bool> AcceptInvitation(Guid invitationId, CancellationToken cancellationToken);

    Task<bool> RefuseInvitation(Guid invitationId, CancellationToken cancellationToken);
    
    Task<IEnumerable<Invitation>> GetInvitationByParticipant(string participantId);
    
    Task<bool> DeleteInvitations(Guid holidayId, CancellationToken cancellationToken);
    
    Task<bool> DeleteInvitationsByParticipant(Guid holidayId, string participantId, CancellationToken cancellationToken);
    
    Task<bool> RemainingParticipantsInHoliday(Guid holidayId, CancellationToken cancellationToken);
}