using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.RaceService.Services;
using Xunit;

namespace ThesisBackend.Services.Tests.Race.Services;

public class RaceServiceTest
{
    private readonly RaceService.Services.RaceService _raceService;
    private readonly dbContext _dbContext;
    private readonly Mock<ILogger<RaceService.Services.RaceService>> _mockLogger;

    public RaceServiceTest()
    {
        var options = new DbContextOptionsBuilder<dbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new dbContext(options);
        _mockLogger = new Mock<ILogger<RaceService.Services.RaceService>>();
        _raceService = new RaceService.Services.RaceService(_dbContext, _mockLogger.Object);
    }

    private async Task<Domain.Models.User> SeedUserAsync()
    {
        var user = new Domain.Models.User { Id = 1, Nickname = "TestUser", Email = "test@example.com", PasswordHash = "hash" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task AddRaceAsync_ShouldAddRace_WhenUserExists()
    {
        var user = await SeedUserAsync();
        var request = new RaceRequest { Name = "Test Race", Description = "A test race", Location = "Test Location", Coordinates = "0,0", RaceType = RaceType.Drag, Date = DateTime.UtcNow.AddDays(1) };

        var result = await _raceService.AddRaceAsync(request, user.Id);

        Assert.True(result.Success);
        Assert.NotNull(result.Race);
        Assert.Equal("Test Race", result.Race.Name);
    }
}