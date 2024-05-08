namespace Holiday.Api.Repository.Repositories;

public interface IAuthRepository
{ 
    Task<Models.Participant> GetUserByEmail(string email) ;
}