using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.CarService.Interfaces;
using ThesisBackend.Services.CarService.Models;

namespace ThesisBackend.Services.CarService.Services;

public class CarService : ICarService
{
    private readonly ILogger<CarService> _logger;
    private readonly dbContext _context;
    
    public CarService(ILogger<CarService> logger, dbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    public async Task<CarOperationResult> addCar(CarRequest carRequest, int userId)
    {
        _logger.LogInformation("Attempting to add a new car for user with ID: {userId}", userId);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {userId} not found.", userId);
            return new CarOperationResult { Success = false, ErrorMessage = "User not found" };
        }
        _logger.LogInformation("User with ID {userId} found. Proceeding to add car.", userId);
        var newCar = new Car(carRequest, user);
        _context.Cars.Add(newCar);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully added new car with ID: {carId} for user with ID: {userId}", newCar.Id, userId);
        return new CarOperationResult { Success = true, CarResponse = new CarResponse(newCar) };
    }
    
    public async Task<CarOperationResult> editCar(CarRequest carRequest, int carId)
    {
        _logger.LogInformation("Attempting to edit car with ID: {carId}", carId);
        var car = await _context.Cars.FindAsync(carId);
        if (car == null)
        {
            _logger.LogWarning("Car with ID {carId} not found.", carId);
            return new CarOperationResult { Success = false, ErrorMessage = "Car not found" };
        }
        _logger.LogInformation("Car with ID {carId} found. Proceeding to update car details.", carId);
        car.UpdateCar(carRequest);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully updated car with ID: {carId}", carId);
        return new CarOperationResult { Success = true, CarResponse = new CarResponse(car) };
    }
    
    public async Task<AllCarOperationResult> getAllCars(int userId)
    {
        _logger.LogInformation("Attempting to retrieve all cars for user with ID: {userId}", userId);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {userId} not found.", userId);
            return new AllCarOperationResult { Success = false, ErrorMessage = "User not found" };
        }
        _logger.LogInformation("User with ID {userId} found. Retrieving cars.", userId);
        var cars = await _context.Cars.Where(car => car.UserId == userId).ToListAsync();
        if (!cars.Any())
        {
            _logger.LogWarning("No cars found for user with ID {userId}.", userId);
            return new AllCarOperationResult { Success = false, ErrorMessage = "No cars found" };
        }
        _logger.LogInformation("Successfully retrieved {carCount} cars for user with ID: {userId}", cars.Count, userId);
        return new AllCarOperationResult { Success = true, CarResponses = cars.Select(car => new CarResponse(car)).ToList() };
    }
    
    public async Task<CarOperationResult> deleteCar(int carId)
    {
        _logger.LogInformation("Attempting to delete car with ID: {carId}", carId);
        var car = await _context.Cars.FindAsync(carId);
        if (car == null)
        {
            _logger.LogWarning("Car with ID {carId} not found.", carId);
            return new CarOperationResult { Success = false, ErrorMessage = "Car not found" };
        }
        _logger.LogInformation("Car with ID {carId} found. Proceeding to delete.", carId);
        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully deleted car with ID: {carId}", carId);
        return new CarOperationResult { Success = true };
    }
}