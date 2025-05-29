using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;

namespace ThesisBackend.Controllers;

[Route("api/v1/Authentication")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly dbContext _context;
	private readonly IConfiguration _configuration;

	public AuthController(dbContext context, IConfiguration configuration)
	{
		_context = context;
		_configuration = configuration;
	}
	
	[HttpPost("register")]
	[AllowAnonymous]
	public async Task<IActionResult> Register(RegistrationRequest registrationRequest)
	{
		//Here comes the validation later on for the validation of the request
		_context.Users.Add(new User(registrationRequest));
		await _context.SaveChangesAsync();
		return Ok(new { message = "User registered successfully" });
	}
	
	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<IActionResult> Login(LoginRequest loginRequest)
	{
		//Here comes the validation later on for the validation of the request
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
		if (user == null)
		{
			return Unauthorized();
		}
		if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
		{
			return Unauthorized();
		}

		SetJWTToken(user);
		return Ok(new LoginResponse(user));
	}

	private void SetJWTToken(User user)
	{
		var token = GenerateJwtToken(user);
		Response.Cookies.Append("accessToken", token, new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Lax,
			Expires = DateTimeOffset.UtcNow.AddMinutes(30),
			Path = "/",
		});
	}
	
	private string GenerateJwtToken(User user)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var secret = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
		var tokenDescriptor = new SecurityTokenDescriptor()
		{
			Subject = new ClaimsIdentity(new Claim[]
			{
				new Claim(ClaimTypes.Name, user.Id.ToString())
			}),
			Expires = DateTime.UtcNow.AddMinutes(30),
			Issuer = _configuration["Jwt:Issuer"], // Set the Issuer
			Audience = _configuration["Jwt:Audience"], // Set the Audience
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature)
		};
		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken(token);
	}
}