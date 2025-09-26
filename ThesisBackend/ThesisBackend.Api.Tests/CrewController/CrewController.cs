// ThesisBackend/ThesisBackend.Api.Tests/CrewController/CrewControllerTests.cs
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
using Xunit;
using Xunit.Abstractions;

namespace ThesisBackend.Api.Tests.CrewController;

public class CrewControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
        private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public CrewControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
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
    public async Task CrewEndpoints_ShouldWorkEndToEnd()
    {
        // Arrange
        var user = await SeedUserAsync();
        _client.DefaultRequestHeaders.Add("User-Id", user.Id.ToString());

        // 1. Create a Crew
        var createRequest = new CrewRequest
        {
            Name = "The Testers", Description = "A crew for testing.", ImageLocation = "image.jpg"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/crews", createRequest);

        if (!createResponse.IsSuccessStatusCode)
        {
            var errorContent = await createResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"API call failed. Status: {createResponse.StatusCode}, Content: {errorContent}");
        }
        createResponse.EnsureSuccessStatusCode();
        
        var createdCrew = await createResponse.Content.ReadFromJsonAsync<CrewResponse>();
        createdCrew.Should().NotBeNull();
        createdCrew!.Name.Should().Be("The Testers");
        
        // 2. Get the Crew
        var getResponse = await _client.GetAsync($"/api/v1/crews/{createdCrew.Id}");
        getResponse.EnsureSuccessStatusCode();
        var fetchedCrew = await getResponse.Content.ReadFromJsonAsync<CrewResponse>();
        fetchedCrew!.Description.Should().Be("A crew for testing.");

        // 3. Update the Crew
        var updateRequest = new CrewRequest { Name = "The Testers Updated", Description = "Updated Desc", ImageLocation = "updated.jpg" };
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/crews/{createdCrew.Id}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        // 4. Verify Update
        var getUpdatedResponse = await _client.GetAsync($"/api/v1/crews/{createdCrew.Id}");
        var updatedCrew = await getUpdatedResponse.Content.ReadFromJsonAsync<CrewResponse>();
        updatedCrew!.Name.Should().Be("The Testers Updated");
        updatedCrew!.Description.Should().Be("Updated Desc");
        updatedCrew!.ImageLocation.Should().Be("updated.jpg");

        // 5. Delete the Crew
        var deleteResponse = await _client.DeleteAsync($"/api/v1/crews/{createdCrew.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        // 6. Verify Deletion
        var getDeletedResponse = await _client.GetAsync($"/api/v1/crews/{createdCrew.Id}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}