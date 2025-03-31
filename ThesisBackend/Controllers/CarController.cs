using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThesisBackend.Data;
using ThesisBackend.Messages;
using ThesisBackend.Models;

namespace ThesisBackend.Controllers;

[Route("api/v1/cars")]
[ApiController]
public class CarController : ControllerBase
{
	private readonly dbContext _context;

	public CarController(dbContext context)
	{
		_context = context;
	}
	
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpPost("addCar/{userId}")]
	public async Task<ActionResult<CarResponse>> AddCar(CarRequest carRequest, int userId)
	{
		//Here comes the validation later on for the validation of the request
		var user = await _context.Users.FindAsync(userId);
		if (user == null)
		{
			return NotFound(new { message = "User not found" });
		}
		_context.Cars.Add(new Car(carRequest, user));
		await _context.SaveChangesAsync();
		var car = await _context.Cars.OrderBy(c => c.Id).LastOrDefaultAsync(c => c.Brand == carRequest.Brand && c.Model == carRequest.Model);
		return Ok(new CarResponse(car));
	}
	
    //[Authorize]	//Uncomment this line to enable authorization
	[HttpPost("updateCar/{carId}")]
	public async Task<ActionResult<CarResponse>> EditCar(CarRequest carRequest, int carId)
	{
	    //Here comes the validation later on for the validation of the request
	    var car = await _context.Cars.FindAsync(carId);
	    if (car == null)
	    {
    		return NotFound(new { message = "Car not found" });
	    }
	    car.UpdateCar(carRequest);
	    await _context.SaveChangesAsync();
	    return Ok(new CarResponse(car));
	}
	
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpGet("getCars/{userId}")]
	public async Task<ActionResult<List<CarResponse>>> GetAllCars(int userId)
	{
		var user = await _context.Users.FindAsync(userId);
		if (user == null)
		{
			return NotFound(new { message = "User not found" });
		}
		var cars = await _context.Cars.Where(c => c.UserId == userId).ToListAsync();
		return Ok(cars.Select(c => new CarResponse(c)).ToList());
	}
	
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpDelete("deleteCar/{carId}")]
	public async Task<ActionResult> DeleteCar(int carId)
    {
	    var car = await _context.Cars.FindAsync(carId);
	    if (car == null)
	    {
		    return NotFound(new { message = "Car not found" });
	    }
	    if (!Request.Cookies.TryGetValue("accessToken", out var token))
	    {
		    return Unauthorized(new { message = "Unauthorized access" });
	    }
	    var tokenHandler = new JwtSecurityTokenHandler();
	    var jwtToken = tokenHandler.ReadJwtToken(token);

	    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name");
	    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var parsedUserId) || car.UserId != parsedUserId)
	    {
		    return Unauthorized(new { message = "Unauthorized access" });
	    }

	    _context.Cars.Remove(car);
	    await _context.SaveChangesAsync();
	    return Ok();
    }
}