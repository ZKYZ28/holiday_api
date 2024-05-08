namespace Holiday.Api.Repository.CustomErrors;

public class RessourceNotFoundException : Exception
{
    public RessourceNotFoundException()
    {
    }
    
    public RessourceNotFoundException(string message)
        : base(message)
    {
    }
    
    public RessourceNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}