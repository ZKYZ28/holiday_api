namespace Holiday.Api.Repository.CustomErrors;

public class HolidayStorageException : Exception
{
    public HolidayStorageException(string msg) : base(msg)
    {
        
    }
}