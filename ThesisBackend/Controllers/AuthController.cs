using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using ThesisBackend.Application.Authentication.Interfaces;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Services.Authentication.Models;

namespace ThesisBackend.Controllers;

[Route("api/v1/Authentication")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IConfiguration _configuration;
	private readonly IAuthService _authService;
	private readonly JwtSettings _jwtSettings;
	
	// FluentValidation validators for the request models
	private readonly IValidator<RegistrationRequest> _registrationRequestValidator; 
	private readonly IValidator<LoginRequest> _loginRequestValidator;

	public AuthController(IAuthService authService, IConfiguration configuration, IOptions<JwtSettings> jwtSettings, 
		IValidator<RegistrationRequest> registrationRequestValidator, IValidator<LoginRequest> loginRequestValidator)
	{
		_authService = authService;
		_configuration = configuration;
		_jwtSettings = jwtSettings.Value;
		_registrationRequestValidator = registrationRequestValidator;
		_loginRequestValidator = loginRequestValidator;
	}
	
	[HttpPost("register")]
	[AllowAnonymous]
	public async Task<IActionResult> Register(RegistrationRequest registrationRequest)
	{
		var validationResult = await _registrationRequestValidator.ValidateAsync(registrationRequest);

		if (!validationResult.IsValid)
		{
			validationResult.AddToModelState(ModelState);
			return ValidationProblem(ModelState); // Returns a 400 Bad Request with ProblemDetails
		}
		
		var result = await _authService.RegisterAsync(registrationRequest);
		if (!result.Success)
		{
			return Problem(
				statusCode: 400,
				title: "Registration Failed", 
				detail: result.ErrorMessage ?? "An error occurred during registration."
			);
		}
		
		return Ok(new { message = "User registered successfully" });
	}
	
	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<IActionResult> Login(LoginRequest loginRequest)
	{
		var validationResult = await _loginRequestValidator.ValidateAsync(loginRequest);
		if (!validationResult.IsValid)
		{
			validationResult.AddToModelState(ModelState);
			return ValidationProblem(ModelState); // Returns a 400 Bad Request with ProblemDetails
		}
		
		var result = await _authService.LoginAsync(loginRequest);

		if (!result.Success || string.IsNullOrEmpty(result.Token) || result.UserResponse == null)
		{
			return Problem(
				statusCode: 401,
				title: "Login Failed",
				detail: result.ErrorMessage ?? "Invalid username or password."
			);
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