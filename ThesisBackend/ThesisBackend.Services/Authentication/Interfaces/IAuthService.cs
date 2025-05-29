using System.Threading.Tasks;
using ThesisBackend.Domain.Messages; 
using ThesisBackend.Application.Authentication.Models;

namespace ThesisBackend.Application.Authentication.Interfaces
{
    public interface IAuthService
    {
        Task<AuthOperationResult> RegisterAsync(RegistrationRequest request);
        Task<AuthOperationResult> LoginAsync(LoginRequest request);
    }
}