namespace Holiday.Api.Repository.CustomErrors;

public class LoadDataBaseException : Exception
{
    public LoadDataBaseException()
    {
    }
    
    public LoadDataBaseException(string message)
        : base(message)
    {
    }
    
    public LoadDataBaseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

}