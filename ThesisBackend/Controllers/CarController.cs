using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.CarService.Interfaces;
using ThesisBackend.Services.CarService.Services;

namespace ThesisBackend.Controllers;

[Route("api/v1/cars")]
[ApiController]
public class CarController : ControllerBase
{
	private readonly dbContext _context;
	private readonly IValidator<CarRequest> _carRequestValidator;
	private readonly ICarService _carService;
	private readonly ILogger<CarController> _logger;
	
	public CarController(dbContext context, ILogger<CarController> logger,
		IValidator<CarRequest> carRequestValidator, ICarService carService)
	{
		_carRequestValidator = carRequestValidator;
		_carService = carService;
		_context = context;
		_logger = logger;
	}
	
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpPost("addCar/{userId}")]
	public async Task<ActionResult<CarResponse>> AddCar(CarRequest carRequest, int userId)
	{
		_logger.LogInformation("Adding car for user {userId}", userId);
		var validationResult = await _carRequestValidator.ValidateAsync(carRequest);
		if (!validationResult.IsValid)
		{
			_logger.LogWarning("Car request validation failed: {errors}", validationResult.Errors);
			return BadRequest(new { message = "Invalid car request", errors = validationResult.Errors });
		}
		if (userId <= 0 || !await _context.Users.AnyAsync(u => u.Id == userId))
		{
			_logger.LogWarning("Invalid userId: {userId}", userId);
			return BadRequest(new { message = "Invalid user ID" });
		}
		_logger.LogInformation("Creating car for user {userId} with request: {@carRequest}", userId, carRequest);
		var result = await _carService.addCar(carRequest, userId);
		
		if (!result.Success || result.CarResponse == null)
		{
			_logger.LogError("Failed to add car for user {userId}: {error}", userId, result.ErrorMessage);
			return Problem(
				statusCode: 400,
				title: "Car Not Added",
				detail: result.ErrorMessage ?? "Car could not be added."
			);
		}
		_logger.LogInformation("Car added successfully for user {userId}: {@carResponse}", userId, result.CarResponse);
		return Ok(result.CarResponse);
	}
	
    //[Authorize]	//Uncomment this line to enable authorization
	[HttpPost("updateCar/{carId}")]
	public async Task<ActionResult<CarResponse>> EditCar(CarRequest carRequest, int carId)
	{
		_logger.LogInformation("Updating car with ID {carId}", carId);
		var validationResult = await _carRequestValidator.ValidateAsync(carRequest);
		if (!validationResult.IsValid)
		{
			_logger.LogWarning("Car request validation failed: {errors}", validationResult.Errors);
			return BadRequest(new {
					message = "Invalid car request",
					errors = validationResult.Errors
			});
		}

		if (carId <= 0 || !await _context.Cars.AnyAsync(c => c.Id == carId))
		{
			_logger.LogWarning("Invalid carId: {carId}", carId);
			return BadRequest(new { message = "Invalid car ID" });
		}
		_logger.LogInformation("Updating car with ID {carId} with request: {@carRequest}", carId, carRequest);
		
		var result = await _carService.editCar(carRequest, carId);
		if (!result.Success || result.CarResponse == null)
		{
			_logger.LogInformation("Failed to update car with ID {carId}: {error}", carId, result.ErrorMessage);
			return Problem(
				statusCode: 400,
				title: "Car Not Updated",
				detail: result.ErrorMessage ?? "Car could not be updated."
			);
		}
		
		_logger.LogInformation("Car with ID {carId} updated successfully: {@carResponse}", carId, result.CarResponse);
		return Ok(result.CarResponse);
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

		var result = await _carService.getAllCars(userId);
		if (!result.Success || result.CarResponses == null || !result.CarResponses.Any())
		{
			_logger.LogWarning("Failed to get all cars for user {userId}", userId);
			return NotFound(new { message = "No cars found for this user" });
		}
		_logger.LogInformation("Successfully retrieved {count} cars for user {userId}", 
			result.CarResponses.Count, userId);
		return Ok(result.CarResponses.ToList());
	}
	
	//[Authorize]	//Uncomment this line to enable authorization
	[HttpDelete("deleteCar/{carId}")]
	public async Task<ActionResult> DeleteCar(int carId)
    {
	    var car = await _context.Cars.FindAsync(carId);
	    if (car == null || !await _context.Cars.AnyAsync(u => u.Id == carId))
	    {
		    return NotFound(new { message = "Car not found" });
	    }
	    _logger.LogInformation("Deleting car with ID {carId}", carId);
	    var result = await _carService.deleteCar(carId);
	    if (!result.Success)
	    {
		    _logger.LogWarning("Failed to delete car with ID {carId}", carId);
		    return NotFound(new { message = "No cars found for this user" });
	    }
	    
	    _logger.LogInformation("Successfully deleted car with ID {carId}", carId);
	    return NoContent();
    }
}