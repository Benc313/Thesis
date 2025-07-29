using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Services.Tests.Car.Services;

public class CarServiceTest
{
    private readonly CarService.Services.CarService _carService;
    private readonly Mock<ILogger<CarService.Services.CarService>> _mockLogger;
    private readonly dbContext _dbContext; // Use InMemory for testing

    public CarServiceTest()
    {
        var options = new DbContextOptionsBuilder<dbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid()
                .ToString()) // Unique DB name for each test class instance
            .Options;
        _dbContext = new dbContext(options);
        _mockLogger = new Mock<ILogger<CarService.Services.CarService>>();
        _carService = new CarService.Services.CarService(_mockLogger.Object, _dbContext);
    }
    
    [Fact]
    public async Task AddCar_ShouldReturnSuccess_WhenCarIsAdded()
    {
        // Arrange
        var user = new Domain.Models.User
        {
            Id = 1,
            Nickname = "TestUser",
            Email = "Test@user.123",
            PasswordHash = "hashed_password"
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        // Create a valid CarRequest
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        int userId = 1; // Assuming user with ID 1 exists

        // Act
        var result = await _carService.addCar(carRequest, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.CarResponse);
        Assert.Equal("Toyota", result.CarResponse.Brand);
    }
    
    [Fact]
    public async Task AddCar_ShouldReturnError_WhenUserDoesNotExist()
    {
        // Arrange
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        int userId = 999; // Non-existent user ID

        // Act
        var result = await _carService.addCar(carRequest, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found", result.ErrorMessage);
    }
    
    [Fact]
    public async Task EditCar_ShouldReturnSuccess_WhenCarIsEdited()
    {
        // Arrange
        var user = new Domain.Models.User
        {
            Id = 1,
            Nickname = "TestUser",
            Email = "Test@user.123",
            PasswordHash = "hashed_password"
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        var addResult = await _carService.addCar(carRequest, user.Id);
        int carId = addResult.CarResponse.Id;

        // Create an updated CarRequest
        var updatedCarRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Corolla",
            Description = "An updated reliable car",
            Engine = "1.8L",
            HorsePower = 140
        };

        // Act
        var editResult = await _carService.editCar(updatedCarRequest, carId);

        // Assert
        Assert.True(editResult.Success);
        Assert.NotNull(editResult.CarResponse);
        Assert.Equal("Toyota", editResult.CarResponse.Brand);
        Assert.Equal("Corolla", editResult.CarResponse.Model);
    }
    
    [Fact]
    public async Task EditCar_ShouldReturnError_WhenCarDoesNotExist()
    {
        // Arrange
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        int carId = 999; // Non-existent car ID

        // Act
        var result = await _carService.editCar(carRequest, carId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Car not found", result.ErrorMessage);
    }
    
    [Fact]
    public async Task GetAllCars_ShouldReturnCars_WhenUserExists()
    {
        // Arrange
        var user = new Domain.Models.User
        {
            Id = 1,
            Nickname = "TestUser",
            Email = "Test@user.123",
            PasswordHash = "hashed_password"
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        await _carService.addCar(carRequest, user.Id);

        // Act
        var result = await _carService.getAllCars(user.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.CarResponses);
    }
    
    [Fact]
    public async Task GetAllCars_ShouldReturnError_WhenUserDoesNotExist()
    {
        // Arrange
        int userId = 999; // Non-existent user ID

        // Act
        var result = await _carService.getAllCars(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteCar_ShouldReturnSuccess_WhenCarIsDeleted()
    {
        // Arrange
        var carId = 1;
        
        var user = new Domain.Models.User
        {
            Id = 1,
            Nickname = "TestUser",
            Email = "Test@user.123",
            PasswordHash = "hashed_password"
        };
        _dbContext.Users.Add(user);
        var car = new Domain.Models.Car
        {
            Id = carId,
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132,
            User = user,
            UserId = user.Id
        };
        _dbContext.Cars.Add(car);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var result = await _carService.deleteCar(carId);
        
        // Assert
        Assert.True(result.Success);
    }
    
    [Fact]
    public async Task DeleteCar_ShouldReturnError_WhenCarDoesNotExist()
    {
        // Arrange
        int carId = 999; // Non-existent car ID

        // Act
        var result = await _carService.deleteCar(carId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Car not found", result.ErrorMessage);
    }
}