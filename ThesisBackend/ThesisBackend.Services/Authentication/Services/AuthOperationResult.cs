namespace ThesisBackend.Application.Authentication.Models
{
    public class AuthOperationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } // Could also be List<string> for multiple errors
        public Domain.Messages.UserResponse UserResponse { get; set; } // Populated on successful login
        public string Token { get; set; } // Populated on successful login
    }
}