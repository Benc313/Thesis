using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ThesisBackend.Application.Authentication.Interfaces;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.Authentication.Models;

namespace ThesisBackend.Controllers;

[Route("api/v1/Authentication")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IConfiguration _configuration;
	private readonly IAuthService _authService;
	private readonly JwtSettings _jwtSettings;

	public AuthController(IAuthService authService, IConfiguration configuration, IOptions<JwtSettings> jwtSettings)
	{
		_authService = authService;
		_configuration = configuration;
		_jwtSettings = jwtSettings.Value;
	}
	
	[HttpPost("register")]
	[AllowAnonymous]
	public async Task<IActionResult> Register(RegistrationRequest registrationRequest)
	{
		var result = await _authService.RegisterAsync(registrationRequest);
		if (!result.Success)
		{
			// Later on a more meaningful error message can be returned
			return BadRequest(new { message = result.ErrorMessage });
		}
		
		return Ok(new { message = "User registered successfully" });
	}
	
	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<IActionResult> Login(LoginRequest loginRequest)
	{
		var result = await _authService.LoginAsync(loginRequest);

		if (!result.Success || string.IsNullOrEmpty(result.Token) || result.UserResponse == null)
		{
			// Re
			return Unauthorized(new { message = result.ErrorMessage ?? "Invalid credentials." });
		}

		// Set the JWT token in an HttpOnly cookie
		Response.Cookies.Append("accessToken", result.Token, new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.Lax,
			Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
			Path = "/" // Cookie available for all paths
		});

		return Ok(result.UserResponse);
	}
}