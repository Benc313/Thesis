using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using Xunit;

namespace ThesisBackend.Api.Tests.RaceController;

public class RaceControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public RaceControllerTests(WebApplicationFactory<Program> factory)
    {
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
    public async Task RaceEndpoints_ShouldWorkEndToEnd()
    {
        var user = await SeedUserAsync();
        _client.DefaultRequestHeaders.Add("User-Id", user.Id.ToString());

        // Create
        var createRequest = new RaceRequest { Name = "Test Race", Description = "Test Description", Location = "Test Location", Coordinates = "0,0", RaceType = RaceType.Drag, Date = DateTime.UtcNow.AddDays(1) };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/races", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdRace = await createResponse.Content.ReadFromJsonAsync<RaceResponse>();
        createdRace.Should().NotBeNull();
        createdRace.Name.Should().Be("Test Race");

        // Get
        var getResponse = await _client.GetAsync($"/api/v1/races/{createdRace.Id}");
        getResponse.EnsureSuccessStatusCode();
        var fetchedRace = await getResponse.Content.ReadFromJsonAsync<RaceResponse>();
        fetchedRace.Description.Should().Be("Test Description");

        // Update
        var updateRequest = new RaceRequest { Name = "Updated Race", Description = "Updated Description", Location = "Updated Location", Coordinates = "1,1", RaceType = RaceType.Circuit, Date = DateTime.UtcNow.AddDays(2) };
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/races/{createdRace.Id}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        // Verify Update
        var getUpdatedResponse = await _client.GetAsync($"/api/v1/races/{createdRace.Id}");
        var updatedRace = await getUpdatedResponse.Content.ReadFromJsonAsync<RaceResponse>();
        updatedRace.Name.Should().Be("Updated Race");

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/v1/races/{createdRace.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        // Verify Deletion
        var getDeletedResponse = await _client.GetAsync($"/api/v1/races/{createdRace.Id}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}