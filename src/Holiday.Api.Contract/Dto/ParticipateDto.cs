using DefaultNamespace;

namespace Holiday.Api.Contract.Dto;

public class ParticipateInDto
{
    public string ActivityId { get; set; }
    public string ParticipantId { get; set; }
}

public class ParticipateOutDto : ParticipateInDto
{
    public string Id { get; set; }
    
    public ParticipantOutDto Participant { get; set; }
    
    public ActivityOutDto Activity { get; set; }
}