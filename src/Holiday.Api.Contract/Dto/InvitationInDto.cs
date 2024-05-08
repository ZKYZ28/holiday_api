using DefaultNamespace;

namespace Holiday.Api.Contract.Dto;

public class InvitationInDto
{
    public string  HolidayId  { get; set; }

    public string ParticipantId { get; set; }
}

public class InvitationOutDto : InvitationInDto
{
    public string Id { get; set; }

    public HolidayOutDto Holiday { get; set; }

    public ParticipantOutDto Participant { get; set; }
}