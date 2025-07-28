using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using Xunit.Abstractions;

namespace ThesisBackend.Api.Tests.CarController;

public class CarControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    
    public CarControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Ensure the real DbContext is removed before adding the in-memory one
                services.RemoveAll(typeof(DbContextOptions<dbContext>));
                
                // Use a unique name for each test class instance to ensure isolation
                var dbName = $"InMemoryDbForTesting_{Guid.NewGuid()}";
                services.AddDbContext<dbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });

        _client = _factory.CreateClient();
    }
    
    private async Task SeedUserAsync(params Domain.Models.User[] users)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<dbContext>();

        // Ensure database is clean for each test (important for IClassFixture where factory is reused)
        await context.Database.EnsureDeletedAsync(); // Add this line
        await context.Database.EnsureCreatedAsync();

        foreach (var user in users)
        {
            context.Users.Add(user);
            _output.WriteLine($"Seeding user with ID: {user.Id} and Nickname: {user.Nickname}");
        }
        await context.SaveChangesAsync();

    }


    [Fact]
    public async Task AddCar_ShouldReturnOk_WhenCarIsAddedSuccessfully()
    {
        // Arrange
        int userId = 1;
        var user = new Domain.Models.User
        {
            Id = userId,
            Nickname = "TestUser",
            Email = "test@user.123",
            PasswordHash = "hashed_password"
        };
        await SeedUserAsync(user);
        var carRequest = new Car
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/addCar/{userId}", carRequest);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var carResponse = await response.Content.ReadFromJsonAsync<CarResponse>();
        Assert.NotNull(carResponse);
        Assert.Equal(carRequest.Brand, carResponse.Brand);
    }
    
    [Fact]
    public async Task AddCar_ShouldReturnBadRequest_WhenUserDoesNotExist()
    {
        // Arrange
        int userId = 999; // Non-existent user ID
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/addCar/{userId}", carRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task AddCar_ShouldReturnBadRequest_WhenCarRequestIsInvalid()
    {
        // Arrange
        int userId = 1;
        var user = new Domain.Models.User
        {
            Id = userId,
            Nickname = "TestUser",
            Email = "test@user.123",
            PasswordHash = "hashed_password"
        };
        await SeedUserAsync(user);
        
        // Invalid car request (missing required fields)
        var carRequest = new CarRequest
        {
            Brand = "", // Invalid brand
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/addCar/{userId}", carRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task AddCar_ShouldReturnProblemDetails_WhenCarCannotBeAdded()
    {
        // Arrange
        int userId = 1;
        var user = new Domain.Models.User
        {
            Id = userId,
            Nickname = "TestUser",
            Email = "test@user.123",
            PasswordHash = "hashed_password"
        };
        await SeedUserAsync(user);
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/addCar/{userId}", ""); // Sending null to simulate failure
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task AddCar_ShouldReturnProblemDetails_WhenUserIdIsInvalid()
    {
        // Arrange
        int userId = -1; // Invalid user ID
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/addCar/{userId}", carRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task AddCar_ShouldReturnProblemDetails_WhenUserIdIsZero()
    {
        // Arrange
        int userId = 0; // Invalid user ID
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/addCar/{userId}", carRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task EditCar_ShouldReturnOk_WhenCarIsEditedSuccessfully()
    {
        // Arrange
        int userId = 1;
        var user = new Domain.Models.User
        {
            Id = userId,
            Nickname = "TestUser",
            Email = "test@user.123",
            PasswordHash = "hashed_password"
        };
        await SeedUserAsync(user);
        
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        // First, add the car
        var addResponse = await _client.PostAsJsonAsync($"/api/v1/cars/addCar/{userId}", carRequest);
        addResponse.EnsureSuccessStatusCode();
        
        // Now, edit the car
        var editCarRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Corolla",
            Description = "An updated reliable car",
            Engine = "1.8L",
            HorsePower = 140
        };
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/updateCar/1", editCarRequest);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var carResponse = await response.Content.ReadFromJsonAsync<CarResponse>();
        Assert.NotNull(carResponse);
        Assert.Equal(editCarRequest.Brand, carResponse.Brand);
    }
    
    [Fact]
    public async Task EditCar_ShouldReturnBadRequest_WhenCarDoesNotExist()
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
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/updateCar/999", carRequest); // Non-existent car ID
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task EditCar_ShouldReturnBadRequest_WhenCarRequestIsInvalid()
    {
        // Arrange
        int userId = 1;
        var user = new Domain.Models.User
        {
            Id = userId,
            Nickname = "TestUser",
            Email = "test@user.123",
            PasswordHash = "hashed_password"
        };
        await SeedUserAsync(user);
        
        // First, add a car to edit
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        await _client.PostAsJsonAsync($"/api/v1/cars/addCar/{userId}", carRequest);
        
        // Invalid car request (missing required fields)
        var invalidCarRequest = new CarRequest
        {
            Brand = "", // Invalid brand
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/updateCar/1", invalidCarRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task EditCar_ShouldReturnProblemDetails_WhenCarCannotBeEdited()
    {
        // Arrange
        int userId = 1;
        var user = new Domain.Models.User
        {
            Id = userId,
            Nickname = "TestUser",
            Email = "test@user.123",
            PasswordHash = "hashed_password"
        };
        await SeedUserAsync(user);
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/updateCar/1", ""); // Sending null to simulate failure
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task EditCar_ShouldReturnProblemDetails_WhenCarIdIsInvalid()
    {
        // Arrange
        int userId = 1;
        var user = new Domain.Models.User
        {
            Id = userId,
            Nickname = "TestUser",
            Email = "test@user.123",
            PasswordHash = "hashed_password"
        };
        await SeedUserAsync(user);
        
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/updateCar/-1", carRequest); // Invalid car ID
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task EditCar_ShouldReturnProblemDetails_WhenCarIdIsZero()
    {
        // Arrange
        int userId = 1;
        var user = new Domain.Models.User
        {
            Id = userId,
            Nickname = "TestUser",
            Email = "test@user.123",
            PasswordHash = "hashed_password"
        };
        await SeedUserAsync(user);
        
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/cars/updateCar/0", carRequest); // Invalid car ID
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }

    [Fact]
    public async Task GetAllCars_ShouldReturnOk_WhenUserHasCars()
    {
        // Arrange
        int userId = 1;
        var user = new Domain.Models.User
        {
            Id = userId,
            Nickname = "TestUser",
            Email = "test@user.123",
            PasswordHash = "hashed_password"
        };
        await SeedUserAsync(user);
        
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        // Add a car for the user
        await _client.PostAsJsonAsync($"/api/v1/cars/addCar/{userId}", carRequest);
        
        // Act
        var response = await _client.GetAsync($"/api/v1/cars/getCars/{userId}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var carsResponse = await response.Content.ReadFromJsonAsync<List<CarResponse>>();
        Assert.NotNull(carsResponse);
        Assert.Single(carsResponse);
    }
    
    [Fact]
    public async Task GetAllCars_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        int userId = 999; // Non-existent user ID
        
        // Act
        var response = await _client.GetAsync($"/api/v1/cars/getAllCars/{userId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task GetAllCars_ShouldReturnProblemDetails_WhenUserIdIsInvalid()
    {
        // Arrange
        int userId = -1; // Invalid user ID
        
        // Act
        var response = await _client.GetAsync($"/api/v1/cars/getAllCars/{userId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task GetAllCars_ShouldReturnProblemDetails_WhenUserIdIsZero()
    {
        // Arrange
        int userId = 0; // Invalid user ID
        
        // Act
        var response = await _client.GetAsync($"/api/v1/cars/getAllCars/{userId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task DeleteCar_ShouldReturnOk_WhenCarIsDeletedSuccessfully()
    {
        // Arrange
        int userId = 1;
        var user = new Domain.Models.User
        {
            Id = userId,
            Nickname = "TestUser",
            Email = "test@user.123",
            PasswordHash = "hashed_password"
        };
        await SeedUserAsync(user);
        
        var carRequest = new CarRequest
        {
            Brand = "Toyota",
            Model = "Celica",
            Description = "A reliable car",
            Engine = "1.8L",
            HorsePower = 132
        };
        
        // Add a car for the user
        var addResponse = await _client.PostAsJsonAsync($"/api/v1/cars/addCar/{userId}", carRequest);
        addResponse.EnsureSuccessStatusCode();
        
        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/v1/cars/deleteCar/1");
        
        // Assert
        deleteResponse.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task DeleteCar_ShouldReturnNotFound_WhenCarDoesNotExist()
    {
        // Arrange
        int carId = 999; // Non-existent car ID
        
        // Act
        var response = await _client.DeleteAsync($"/api/v1/cars/deleteCar/{carId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task DeleteCar_ShouldReturnProblemDetails_WhenCarIdIsInvalid()
    {
        // Arrange
        int carId = -1; // Invalid car ID
        
        // Act
        var response = await _client.DeleteAsync($"/api/v1/cars/deleteCar/{carId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task DeleteCar_ShouldReturnProblemDetails_WhenCarIdIsZero()
    {
        // Arrange
        int carId = 0; // Invalid car ID
        
        // Act
        var response = await _client.DeleteAsync($"/api/v1/cars/deleteCar/{carId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(problemDetails);
    }
    
    // Additional tests can be added here for more coverage
}