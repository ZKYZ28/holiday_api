namespace Holiday.Api.Repository.Models;

public class AuthResult
{
    public string Token { get; set; }
    public bool Result { get; set; }
    public List<string> Errors { get; set; }
}