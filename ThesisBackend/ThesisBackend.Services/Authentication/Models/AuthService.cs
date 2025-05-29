using Microsoft.EntityFrameworkCore;
using ThesisBackend.Application.Authentication.Interfaces; // For IAuthService, IPasswordHasher, ITokenGenerator
using ThesisBackend.Application.Authentication.Models;    // For AuthOperationResult
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

        public AuthService(
            dbContext context,
            IPasswordHasher passwordHasher,
            ITokenGenerator tokenGenerator)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<AuthOperationResult> RegisterAsync(RegistrationRequest userRequest)
        {
            //Validate the request
            
            var user = new User(userRequest);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userForResponse = new UserResponse(user);

            return new AuthOperationResult { Success = true, UserResponse = userForResponse };
        }

        public async Task<AuthOperationResult> LoginAsync(LoginRequest request)
        {
            //Validation

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return new AuthOperationResult { Success = false, ErrorMessage = "Invalid email or password." };
            }

            var token = _tokenGenerator.GenerateAccessToken(user);

            var userForResponse = new UserResponse(user);

            return new AuthOperationResult { Success = true, UserResponse = userForResponse, Token = token };
        }
    }
}