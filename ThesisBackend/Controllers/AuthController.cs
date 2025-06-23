using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ThesisBackend.Application.Authentication.Interfaces;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Services.Authentication.Models;

namespace ThesisBackend.Controllers;

[Route("api/v1/Authentication")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;
	private readonly JwtSettings _jwtSettings;
	
	// FluentValidation validators for the request models
	private readonly IValidator<RegistrationRequest> _registrationRequestValidator; 
	private readonly IValidator<LoginRequest> _loginRequestValidator;

	private readonly ILogger<AuthController> _logger;
	
	public AuthController(IAuthService authService, IOptions<JwtSettings> jwtSettings, 
		IValidator<RegistrationRequest> registrationRequestValidator, IValidator<LoginRequest> loginRequestValidator,
		ILogger<AuthController> logger)
	{
		_logger = logger;
		_authService = authService;
		_jwtSettings = jwtSettings.Value;
		_registrationRequestValidator = registrationRequestValidator;
		_loginRequestValidator = loginRequestValidator;
	}
	
	[HttpPost("register")]
	[AllowAnonymous]
	public async Task<IActionResult> Register(RegistrationRequest registrationRequest)
	{
		_logger.LogInformation("Received registration request for user: {nickname} with email: {email}",
			registrationRequest.Nickname, registrationRequest.Email);
		var validationResult = await _registrationRequestValidator.ValidateAsync(registrationRequest);

		if (!validationResult.IsValid)
		{
			validationResult.AddToModelState(ModelState);
			_logger.LogWarning("Registration request validation failed for user: {nickname} with email: {email}, with errors: {errors}",
				registrationRequest.Nickname, registrationRequest.Email, validationResult.ToDictionary());
			return ValidationProblem(ModelState); // Returns a 400 Bad Request with ProblemDetails
		}
		
		var result = await _authService.RegisterAsync(registrationRequest);
		if (!result.Success)
		{
			_logger.LogError("Registration failed for user: {nickname} with email: {email}, error: {error}",
				registrationRequest.Nickname, registrationRequest.Email, result.ErrorMessage);
			return Problem(
				statusCode: 400,
				title: "Registration Failed", 
				detail: result.ErrorMessage ?? "An error occurred during registration."
			);
		}
		_logger.LogInformation("User registered successfully: {nickname} with email: {email}",
			registrationRequest.Nickname, registrationRequest.Email);
		return Ok(new { message = "User registered successfully" });
	}
	
	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<IActionResult> Login(LoginRequest loginRequest)
	{
		_logger.LogInformation("Received login request for email: {email}", loginRequest.Email);
		var validationResult = await _loginRequestValidator.ValidateAsync(loginRequest);
		if (!validationResult.IsValid)
		{
			_logger.LogWarning("Login request validation failed for email: {email}, with errors: {errors}",
				loginRequest.Email, validationResult.ToDictionary());
			validationResult.AddToModelState(ModelState);
			return ValidationProblem(ModelState); // Returns a 400 Bad Request with ProblemDetails
		}
		_logger.LogDebug("Validation passed for login request with email: {email}", loginRequest.Email);
		var result = await _authService.LoginAsync(loginRequest);

		if (!result.Success || string.IsNullOrEmpty(result.Token) || result.UserResponse == null)
		{
			_logger.LogWarning("Login failed for email: {email}, error: {error}",
				loginRequest.Email, result.ErrorMessage);
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
		_logger.LogInformation("User logged in successfully: {nickname} with email: {email}",
			result.UserResponse.Nickname, result.UserResponse.Email);
		return Ok(result.UserResponse);
	}
}