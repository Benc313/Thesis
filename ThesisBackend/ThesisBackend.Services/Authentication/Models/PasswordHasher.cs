using ThesisBackend.Services.Authentication.Interfaces;

namespace ThesisBackend.Services.Authentication.Models;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            // Throw something meaningful here if needed, or just return an empty string
            return string.Empty; 
        }
        // BCrypt.Net.BCrypt.HashPassword will generate a salt and hash the password
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
        {
            return false;
        }
        // BCrypt.Net.BCrypt.Verify will extract the salt from the hashedPassword and verify
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}