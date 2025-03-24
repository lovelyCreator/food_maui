namespace Food_maui.Services
{
    public interface IAuthenticationService
    {
        Task<bool> Authenticate(string username, string password);
    }
}