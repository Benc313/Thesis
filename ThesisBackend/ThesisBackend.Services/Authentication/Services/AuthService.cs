using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThesisBackend.Application.Authentication.Interfaces; // For IAuthService, IPasswordHasher, ITokenGenerator
using ThesisBackend.Services.Authentication.Models;    // For AuthOperationResult
using ThesisBackend.Data;                                 // For dbContext
using ThesisBackend.Domain.Messages;                      // For RegistrationRequest, LoginRequest
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.Authentication.Interfaces; // For User

namespace ThesisBackend.Application.Authentication.Services
{
    public class AuthService : IAuthService
    {
        private readonly dbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ILogger<AuthService> _logger;
        public AuthService(
            dbContext context,
            IPasswordHasher passwordHasher,
            ITokenGenerator tokenGenerator,
            ILogger<AuthService> logger)
        {
            _logger = logger;
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<AuthOperationResult> RegisterAsync(RegistrationRequest userRequest)
        {
            _logger.LogInformation("Attempting to register user with Email: {email}, Nickname: {nickname}",
                userRequest.Email, userRequest.Nickname);
            
            _logger.LogDebug("Hashing password for user {nickname}.", userRequest.Nickname);
            var hashedPassword = _passwordHasher.HashPassword(userRequest.Password);
            var user = new User(userRequest, hashedPassword);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {nickname} (ID: {userid}) registered successfully.",
                user.Nickname, user.Id);
            
            var userForResponse = new UserResponse(user);

            return new AuthOperationResult { Success = true, UserResponse = userForResponse };
        }

        public async Task<AuthOperationResult> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("Attempting to login with email {email}.", request.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email {email}. User not found or password mismatch.", request.Email);
                return new AuthOperationResult { Success = false, ErrorMessage = "Invalid email or password." };
            }
            _logger.LogDebug("Password verification successful for UserID: {userid}.", user.Id);

            _logger.LogInformation("Initiating token generation for UserID: {userid}.", user.Id);
            var token = _tokenGenerator.GenerateAccessToken(user);
            _logger.LogInformation("Token generation successful for UserID: {userid}.", user.Id);
            
            var userForResponse = new UserResponse(user);

            return new AuthOperationResult { Success = true, UserResponse = userForResponse, Token = token };
        }
    }
}