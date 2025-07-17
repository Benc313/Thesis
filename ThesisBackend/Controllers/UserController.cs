using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThesisBackend.Application.UserService.Interfaces;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Controllers;
[Route("api/v1/User")]
[ApiController]
public class UserController : ControllerBase
{
	private readonly IUserService _userService;
	private readonly dbContext _context;

	private readonly IValidator<UserRequest> _userRequestValidator;
	
	private readonly ILogger<UserController> _logger;
	
	public UserController(dbContext context, IUserService userService, 
		IValidator<UserRequest> userRequestValidator, ILogger<UserController> logger)
	{
		_logger = logger;
		_userService = userService;
		_userRequestValidator = userRequestValidator;
		_context = context;
	}

	//[Authorize]	//Uncomment this line to enable authorization
	[HttpGet("getUser/{userId}")]
	public async Task<ActionResult<UserResponse>> GetUser(int userId)
	{
		_logger.LogInformation("Get User {userId}", userId);
		if (userId <= 0)
		{
			_logger.LogWarning("Invalid userId: {userId}", userId);
			return BadRequest(new { message = "Invalid user ID" });
		}

		var result = await _userService.GetUser(userId);
		
		if (!result.Success || result.UserResponse == null)
		{
			_logger.LogError("Failed to retrieve user with ID {userId}: {error}", userId, result.ErrorMessage);
			return Problem(
				statusCode: 400,
				title: "User Not Found",
				detail: result.ErrorMessage ?? "User not found."
			);
		}
		
		return result.UserResponse;
	}
	
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpGet("getAllUsers")]
	public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
	{
		_logger.LogInformation("Retrieving all users");

		var result = await _userService.GetUsers();
		if (!result.Success || result.UserResponse == null || !result.UserResponse.Any())
		{
			_logger.LogWarning("No users found or retrieval failed: {error}", result.ErrorMessage);
			return NotFound(new { message = "No users found" });
		}
		_logger.LogInformation("Successfully retrieved {count} users", result.UserResponse.Count);
		return Ok(result.UserResponse.ToList());
	}
	
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpPut("updateUser/{userId}")]
	public async Task<ActionResult<UserResponse>> UpdateUser(UserRequest userRequest, int userId)
	{
		_logger.LogInformation("Updating user with ID {userId}", userId);
		
		var validationResult = await _userRequestValidator.ValidateAsync(userRequest);
		if (!validationResult.IsValid)
		{
			_logger.LogWarning("Validation failed for user update request: {errors}", validationResult.ToDictionary());
			validationResult.AddToModelState(ModelState);
			return ValidationProblem(ModelState); // Returns a 400 Bad Request with ProblemDetails
		}
		
		var result = await _userService.UpdateUser(userRequest, userId);	
		if (!result.Success || result.UserResponse == null)
		{
			_logger.LogError("Failed to update user with ID {userId}: {error}", userId, result.ErrorMessage);
			return Problem(
				statusCode: 400,
				title: "User Update Failed",
				detail: result.ErrorMessage ?? "An error occurred while updating the user."
			);
		}
		_logger.LogInformation("User with ID {userId} updated successfully", userId);
		return Ok(result.UserResponse);
	}
	
	//Authorize]	//Uncomment this line to enable authorization
	[HttpGet("GetUserMeets/{userId}")]
	public async Task<ActionResult<List<SmallEventResponse>?>> GetUserMeets(int userId)
	{
		_logger.LogInformation("Retrieving meets for user with ID {userId}", userId);
		if (userId <= 0)
		{
			_logger.LogWarning("Invalid userId: {userId}", userId);
			return BadRequest(new { message = "Invalid user ID" });
		}
		var result = await _userService.GetUserMeets(userId);
		if (!result.Success || result.SmallEventResponse == null || !result.SmallEventResponse.Any())
		{
			_logger.LogWarning("No meets found for user with ID {userId}: {error}", userId, result.ErrorMessage);
			return NotFound(new { message = "No events found for this user" });
		}
		_logger.LogInformation("Successfully retrieved {count} meets for user with ID {userId}", 
			result.SmallEventResponse.Count, userId);
		return Ok(result.SmallEventResponse.ToList());
	}
	
	//Authorize]	//Uncomment this line to enable authorization
	[HttpGet("GetUserRaces/{userId}")]
	public async Task<ActionResult<List<SmallEventResponse>?>> GetUserRaces(int userId)
	{
		_logger.LogInformation("Retrieving races for user with ID {userId}", userId);
		if (userId <= 0)
		{
			_logger.LogWarning("Invalid userId: {userId}", userId);
			return BadRequest(new { message = "Invalid user ID" });
		}

		var result = await _userService.GetUserRaces(userId);
		if (!result.Success || result.SmallEventResponse == null || !result.SmallEventResponse.Any())
		{
			_logger.LogWarning("No meets found for user with ID {userId}: {error}", userId, result.ErrorMessage);
			return NotFound(new { message = "No events found for this user" });
		}

		_logger.LogInformation("Successfully retrieved {count} meets for user with ID {userId}",
			result.SmallEventResponse.Count, userId);
		return Ok(result.SmallEventResponse.ToList());
	}
}