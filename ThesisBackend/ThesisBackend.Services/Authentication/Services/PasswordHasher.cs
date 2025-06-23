using Microsoft.Extensions.Logging;
using ThesisBackend.Services.Authentication.Interfaces;

namespace ThesisBackend.Services.Authentication.Models;

public class PasswordHasher : IPasswordHasher
{
    private readonly ILogger<PasswordHasher> _logger;

    public PasswordHasher(ILogger<PasswordHasher> logger)
    {
        _logger = logger;
    }
    public string HashPassword(string password)
    {
        _logger.LogDebug("Hashing password.");
        if (string.IsNullOrEmpty(password))
        {
            // Throw something meaningful here if needed, or just return an empty string
            _logger.LogDebug("Failed to hash password: Password is null or empty.");
            return string.Empty; 
        }
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        _logger.LogDebug("Password hashed successfully.");
        return hashedPassword;
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        _logger.LogDebug("Verifying password.");
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
        {
            _logger.LogDebug("Failed to verify password: Password or hashed password is null or empty.");
            return false;
        }
        var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        _logger.LogDebug("Password verification result: {VerificationResult}", isValid);
        return isValid;
    }
}