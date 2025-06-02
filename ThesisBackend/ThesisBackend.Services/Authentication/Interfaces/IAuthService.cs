using ThesisBackend.Domain.Messages; 
using ThesisBackend.Services.Authentication.Models;

namespace ThesisBackend.Application.Authentication.Interfaces
{
    public interface IAuthService
    {
        Task<AuthOperationResult> RegisterAsync(RegistrationRequest request);
        Task<AuthOperationResult> LoginAsync(LoginRequest request);
    }
}