using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThesisBackend.Data;
using ThesisBackend.Messages;
using ThesisBackend.Models;

namespace ThesisBackend.Controllers;

[Route("api/v1/Meet")]
[ApiController]
public class MeetController : ControllerBase
{
	private readonly dbContext _context;

	public MeetController(dbContext context)
	{
		_context = context;
	}
	
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpPost("addMeet/{userId}")]
	public async Task<ActionResult<MeetResponse>> AddMeet(MeetRequest meetRequest, int userId)
	{
		//Here comes the validation later on for the validation of the request
		var user = await _context.Users.FindAsync(userId);
		if (user == null)
		{
			return NotFound(new { message = "User not found" });
		}
		var crew = await _context.Crews.FindAsync(meetRequest.CrewId);
		var newMeet = new Meet(meetRequest, user, crew);
		_context.Meets.Add(newMeet);
		await _context.SaveChangesAsync();
		return Ok(new MeetResponse(newMeet));
	}
	
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpPut("updateMeet/{meetId}")]
	public async Task<ActionResult<MeetResponse>> EditMeet(MeetRequest meetRequest, int meetId)
	{
		//Here comes the validation later on for the validation of the request
		var meet = await _context.Meets.FindAsync(meetId);
		if (meet == null)
		{
			return NotFound(new { message = "Meet not found" });
		}
		var crew = await _context.Crews.FindAsync(meetRequest.CrewId);
		if (crew == null)
		{
			return NotFound(new { message = "Crew not found" });
		}
		meet.UpdateMeet(meetRequest, crew);
		await _context.SaveChangesAsync();
		return Ok(new MeetResponse(meet));
	}
	
	//Authorize]	//Uncomment this line to enable authorization
	[HttpGet("getMeet/{meetId}")]
	public async Task<ActionResult<MeetResponse>> GetMeet(int meetId)
	{
		var meet = await _context.Meets.FindAsync(meetId);
		if (meet == null)
		{
			return NotFound(new { message = "Meet not found" });
		}
		return Ok(new MeetResponse(meet));
	}

	//[Authorize]	//Uncomment this line to enable authorization
	[HttpDelete("deleteMeet/{meetId}")]
	public async Task<ActionResult> DeleteMeet(int meetId)
	{
		var meet = await _context.Meets.FindAsync(meetId);
		if (meet == null)
		{
			return NotFound(new { message = "Meet not found" });
		}
		_context.Meets.Remove(meet);
		await _context.SaveChangesAsync();
		return Ok(new { message = "Meet deleted successfully" });
	}

	[HttpGet("getMeets")]
	public async Task<ActionResult<List<SmallEventResponse>>> GetAllMeets()
	{
		var meets = await _context.Meets
			.ToListAsync();
		return Ok(meets.Select(meet => new SmallEventResponse(meet)).ToList());	
	}

	//[Authorize]	//Uncomment this line to enable authorization
	[HttpGet("getMeetsF")]
	public async Task<ActionResult<List<SmallEventResponse>>> GetAllMeetsWithFilter([FromQuery] LocationQuery query)
	{
		// Input validation
		if (query.Latitude < -90 || query.Latitude > 90 ||
		    query.Longitude < -180 || query.Longitude > 180 ||
		    query.DistanceInKm <= 0)
		{
			return BadRequest(new { message = "Invalid location parameters" });
		}

		// Fetch meets without Include since Tags is not a navigation property
		var meets = await _context.Meets
			.ToListAsync();

		// Filter in memory using the NotMapped properties and Tags
		var filteredMeets = meets
			.Where(m => 
			{
				try
				{
					return CalculateDistance(m.Latitude, m.Longitude, 
						       query.Latitude, query.Longitude) <= query.DistanceInKm &&
					       (query.Tags.Count == 0 || m.Tags.Any(t => query.Tags.Contains(t.ToString())) && m.Date >= DateTime.Today);
				}
				catch
				{
					return false; // Skip if coordinates can't be parsed
				}
			})
			.ToList();

		if (filteredMeets == null || filteredMeets.Count == 0)
		{
			return NotFound(new { message = "No meets found" });
		}

		return Ok(filteredMeets.Select(meet => new SmallEventResponse(meet)).ToList());
	}

	private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
	{
	    const double R = 6371; // Earth's radius in kilometers
	    
	    var dLat = ToRadians(lat2 - lat1);
	    var dLon = ToRadians(lon2 - lon1);
	    
	    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
	            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
	            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
	    
	    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
	    return R * c;
	}

	private double ToRadians(double degrees)
	{
	    return degrees * Math.PI / 180;
	}

	
}