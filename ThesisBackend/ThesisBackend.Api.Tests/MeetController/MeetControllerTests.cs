using System.Net;
using System.Net.Http.Json;
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
using FluentAssertions;

namespace ThesisBackend.Api.Tests.MeetController;

public class MeetControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public MeetControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<dbContext>));
                var dbName = $"InMemoryDbForTesting_{Guid.NewGuid()}";
                services.AddDbContext<dbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });

        _client = _factory.CreateClient();
    }

    private async Task<Domain.Models.User> SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<dbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var user = new Domain.Models.User { Id = 1, Nickname = "TestUser", Email = "test@example.com", PasswordHash = "hash" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task AddMeet_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var user = await SeedUserAsync();
        var meetRequest = new MeetRequest
        {
            Name = "API Test Meet",
            Description = "Description",
            Location = "API Test Location",
            Coordinates = "47.4979,19.0402",
            Date = DateTime.UtcNow.AddHours(25),
            Tags = new List<MeetTags> { MeetTags.Offroad },
            Private = false
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/Meet/addMeet/{user.Id}", meetRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var meetResponse = await response.Content.ReadFromJsonAsync<MeetResponse>();
        meetResponse.Should().NotBeNull();
        meetResponse?.Name.Should().Be("API Test Meet");
    }

    [Fact]
    public async Task GetMeet_ShouldReturnMeet_WhenExists()
    {
        // Arrange
        var user = await SeedUserAsync();
        var meetRequest = new MeetRequest
        {
            Name = "Gettable Meet",
            Location = "Location",
            Description = "Description",
            Coordinates = "47.4979,19.0402",
            Date = DateTime.UtcNow.AddHours(25),
            Tags = new List<MeetTags> { MeetTags.Show }
        };
        var addResponse = await _client.PostAsJsonAsync($"/api/v1/Meet/addMeet/{user.Id}", meetRequest);
        var addedMeet = await addResponse.Content.ReadFromJsonAsync<MeetResponse>();
        Assert.NotNull(addedMeet);

        // Act
        var getResponse = await _client.GetAsync($"/api/v1/Meet/getMeet/{addedMeet.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedMeet = await getResponse.Content.ReadFromJsonAsync<MeetResponse>();
        fetchedMeet.Should().NotBeNull();
        fetchedMeet?.Name.Should().Be("Gettable Meet");
    }
    
    [Fact]
    public async Task DeleteMeet_ShouldReturnOk_WhenMeetExists()
    {
        // Arrange
        var user = await SeedUserAsync();
        var meetRequest = new MeetRequest
        {
            Name = "Deletable Meet",
            Description = "Description",
            Location = "Location",
            Coordinates = "47.4979,19.0402",
            Date = DateTime.UtcNow.AddHours(25),
            Tags = new List<MeetTags> { MeetTags.Show }
        };
        var addResponse = await _client.PostAsJsonAsync($"/api/v1/Meet/addMeet/{user.Id}", meetRequest);
        var addedMeet = await addResponse.Content.ReadFromJsonAsync<MeetResponse>();
        Assert.NotNull(addedMeet);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/v1/Meet/deleteMeet/{addedMeet.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify it's actually deleted
        var getResponse = await _client.GetAsync($"/api/v1/Meet/getMeet/{addedMeet.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}