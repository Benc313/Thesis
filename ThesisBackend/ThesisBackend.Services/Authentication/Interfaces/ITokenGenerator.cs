namespace ThesisBackend.Application.Authentication.Interfaces
{
    public interface ITokenGenerator
    {
        // Assuming your User model is in ThesisBackend.Domain.Models
        string GenerateAccessToken(Domain.Models.User user); 
    }
}