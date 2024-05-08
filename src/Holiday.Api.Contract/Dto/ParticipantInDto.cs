namespace DefaultNamespace;

public class ParticipantInDto
{
    public string LastName { get; set; }
    
    public string FirstName { get; set; }
    
    public string Email { get; set; }
    
}

public class ParticipantOutDto : ParticipantInDto
{
    public string Id { get; set; }
}