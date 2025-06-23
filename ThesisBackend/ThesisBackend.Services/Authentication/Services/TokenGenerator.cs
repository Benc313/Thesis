using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Xml.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ThesisBackend.Application.Authentication.Interfaces;
using ThesisBackend.Domain.Models;

namespace ThesisBackend.Services.Authentication.Models;

public class TokenGenerator : ITokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<TokenGenerator> _logger;
    
    public TokenGenerator(IOptions<JwtSettings> jwtSettings, ILogger<TokenGenerator> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public string GenerateAccessToken(User user)
    {
        _logger.LogInformation("Generating JWT for UserID: {userid}, Email: {email}", user.Id, user.Email);
        var tokenHandler = new JwtSecurityTokenHandler();
        var secret = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        _logger.LogInformation("JWT successfully generated for UserID: {UserId}.", user.Id);;
        return tokenHandler.WriteToken(token);
    }
}