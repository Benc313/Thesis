using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using Xunit;
using Xunit.Abstractions;

namespace ThesisBackend.Api.Tests.User;

public class UserControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public UserControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
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

    /// <summary>
    /// Seeds the in-memory database with a user for testing.
    /// This creates a fresh scope to ensure it uses the same service provider as the test server.
    /// </summary>
    private async Task SeedUserAsync(params Domain.Models.User[] users)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<dbContext>();
        
        // Ensure the database is created for each test, which is important for in-memory databases.
        await context.Database.EnsureCreatedAsync();

        foreach (var user in users)
        {
            context.Users.Add(user);
            _output.WriteLine($"Seeding user with ID: {user.Id} and Nickname: {user.Nickname}");
        }
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetUser_ShouldReturnOkAndUser_WhenUserExists()
    {
        // Arrange
        var testUser = new Domain.Models.User { Id = 1, Nickname = "TestUser1", Email = "test1@example.com", PasswordHash = "some_hash" };
        await SeedUserAsync(testUser);

        // Act
        var response = await _client.GetAsync($"/api/v1/User/getUser/{testUser.Id}");

        // Assert
        response.EnsureSuccessStatusCode(); // Throws if status is not 2xx
        var userResponse = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(userResponse);
        Assert.Equal(testUser.Id.ToString(), userResponse.Id);
        Assert.Equal(testUser.Nickname, userResponse.Nickname);
    }

    [Fact]
    public async Task GetUser_ShouldReturnProblemDetails_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = 999;

        // Act
        var response = await _client.GetAsync($"/api/v1/User/getUser/{nonExistentUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("User Not Found", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnOkAndAllUsers()
    {
        // Arrange
        await SeedUserAsync(
            new Domain.Models.User { Id = 10, Nickname = "MultiUser1", Email = "multi1@example.com", PasswordHash = "some_hash" },
            new Domain.Models.User { Id = 11, Nickname = "MultiUser2", Email = "multi2@example.com", PasswordHash = "some_hash" }
        );

        // Act
        var response = await _client.GetAsync("/api/v1/User/getAllUsers");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
        Assert.NotNull(users);
        Assert.Equal(2, users.Count);
    }
    
    [Fact]
    public async Task UpdateUser_ShouldReturnOk_AndModifyUser()
    {
        // Arrange
        var originalUser = new Domain.Models.User { Id = 20, Nickname = "OriginalNick", Email = "original@example.com", PasswordHash = "some_hash" };
        await SeedUserAsync(originalUser);

        var updateUserRequest = new UserRequest
        {
            Nickname = "UpdatedNick",
            Email = "original@example.com",
            Description = "This is the updated description."
        };
        var content = new StringContent(JsonSerializer.Serialize(updateUserRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/v1/User/updateUser/{originalUser.Id}", content);

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify the response body
        var updatedResponse = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(updatedResponse);
        Assert.Equal("UpdatedNick", updatedResponse.Nickname);
        Assert.Equal("This is the updated description.", updatedResponse.Description);

        // Verify the change in the database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<dbContext>();
        var userInDb = await context.Users.FindAsync(originalUser.Id);
        Assert.NotNull(userInDb);
        Assert.Equal("UpdatedNick", userInDb.Nickname);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var user = new Domain.Models.User { Id = 30, Nickname = "ValidationUser", Email = "validation@example.com", PasswordHash = "some_hash" };
        await SeedUserAsync(user);

        // Create an invalid request (e.g., Nickname is empty)
        var invalidRequest = new UserRequest
        {
            Nickname = "", // Invalid
            Email = "validation@example.com",
            Description = "A valid description."
        };
        var content = new StringContent(JsonSerializer.Serialize(invalidRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/v1/User/updateUser/{user.Id}", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
