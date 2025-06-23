using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Controllers;
[Route("api/v1/User")]
[ApiController]
public class UserController : ControllerBase
{
	private readonly dbContext _context;

	public UserController(dbContext context)
	{
		_context = context;
	}
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpGet("getUser/{userId}")]
	public async Task<ActionResult<UserResponse>> GetUser(int userId)
	{
		var user = await _context.Users.FindAsync(userId);
		if (user == null)
		{
			return NotFound(new { message = "User not found" });
		}
		return Ok(new UserResponse(user));
	}
	
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpGet("getAllUsers")]
	public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
	{
		var users = await _context.Users.ToListAsync();
		if (users == null || users.Count == 0)
		{
			return NotFound(new { message = "No users found" });
		}
		return Ok(users.Select(user => new UserResponse(user)).ToList());
	}
	
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpPut("updateUser/{userId}")]
	public async Task<ActionResult<UserResponse>> UpdateUser(UserRequest userRequest, int userId)
	{
		var user = await _context.Users.FindAsync(userId);
		if (user == null)
		{
			return NotFound(new { message = "User not found" });
		}
		user.UpdateUser(userRequest);
		await _context.SaveChangesAsync();
		return Ok(new UserResponse(user));
	}
	
	//Authorize]	//Uncomment this line to enable authorization
	[HttpGet("GetUserMeets/{userId}")]
	public async Task<ActionResult<List<SmallEventResponse>?>> GetUserMeets(int userId)
	{
		var user = await _context.Users.FindAsync(userId);
		if (user == null)
		{
			return NotFound(new { message = "User not found" });
		}
		var events = await _context.Meets
			.Include(m => m.Users)
			.Where(m => m.Users.Any(u => u.Id == userId) || m.CreatorId == userId)
			.ToListAsync();
		if (events == null)
			return NotFound(new { message = "No events found for this user" });
		return Ok(events.Select(e => new SmallEventResponse(e)).ToList());
	}
	
	//Authorize]	//Uncomment this line to enable authorization
	[HttpGet("GetUserRaces/{userId}")]
	public async Task<ActionResult<List<SmallEventResponse>?>> GetUserRaces(int userId)
	{
		var user = await _context.Users.FindAsync(userId);
		if (user == null)
		{
			return NotFound(new { message = "User not found" });
		}
		var events = await _context.Races
			.Where(m => m.Users.Any(u => u.Id == userId))
			.ToListAsync();
		if (events == null)
			return NotFound(new { message = "No events found for this user" });
		return Ok(events.Select(e => new SmallEventResponse(e)).ToList());
	}
}