using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.MeetService;
using Xunit;

namespace ThesisBackend.Services.Tests.Meet.Services;

public class MeetServiceTest
{
    private readonly MeetService.Services.MeetService _meetService;
    private readonly Mock<ILogger<MeetService.Services.MeetService>> _mockLogger;
    private readonly dbContext _dbContext;

    public MeetServiceTest()
    {
        var options = new DbContextOptionsBuilder<dbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        _dbContext = new dbContext(options);
        _mockLogger = new Mock<ILogger<MeetService.Services.MeetService>>();
        _meetService = new MeetService.Services.MeetService(_dbContext, _mockLogger.Object);
    }

    private async Task<Domain.Models.User> SeedUserAsync()
    {
        var user = new Domain.Models.User { Id = 1, Nickname = "TestUser", Email = "test@example.com", PasswordHash = "hash" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task AddMeetAsync_ShouldAddMeet_WhenUserExists()
    {
        // Arrange
        var user = await SeedUserAsync();
        var meetRequest = new MeetRequest
        {
            Name = "Test Meet",
            Location = "Test Location",
            Description = "Test Description",
            Coordinates = "47.4979,19.0402",
            Date = DateTime.UtcNow.AddDays(1),
            Tags = new List<MeetTags> { MeetTags.Drift }
        };

        // Act
        var result = await _meetService.AddMeetAsync(meetRequest, user.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.MeetResponse);
        Assert.Equal("Test Meet", result.MeetResponse.Name);
        var meetInDb = await _dbContext.Meets.FirstOrDefaultAsync();
        Assert.NotNull(meetInDb);
        Assert.Equal("Test Meet", meetInDb.Name);
    }

    [Fact]
    public async Task GetMeetAsync_ShouldReturnMeet_WhenMeetExists()
    {
        // Arrange
        var user = await SeedUserAsync();
        var meet = new Domain.Models.Meet(new MeetRequest
        {
            Name = "Existing Meet",
            Location = "Location",
            Description = "Test Description",
            Coordinates = "47.4979,19.0402",
            Date = DateTime.UtcNow.AddDays(1),
            Tags = new List<MeetTags> { MeetTags.Show }
        }, user, null);
        _dbContext.Meets.Add(meet);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _meetService.GetMeetAsync(meet.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.MeetResponse);
        Assert.Equal(meet.Id, result.MeetResponse.Id);
    }
    
    [Fact]
    public async Task DeleteMeetAsync_ShouldDeleteMeet_WhenMeetExists()
    {
        // Arrange
        var user = await SeedUserAsync();
        var meet = new Domain.Models.Meet(new MeetRequest
        {
            Name = "To Be Deleted",
            Location = "Location",
            Description = "Test Description",
            Coordinates = "47.4979,19.0402",
            Date = DateTime.UtcNow.AddDays(1),
            Tags = new List<MeetTags> { MeetTags.Show }
        }, user, null);
        _dbContext.Meets.Add(meet);
        await _dbContext.SaveChangesAsync();
        var meetIdToDelete = meet.Id;

        // Act
        var result = await _meetService.DeleteMeetAsync(meetIdToDelete);

        // Assert
        Assert.True(result.Success);
        var meetInDb = await _dbContext.Meets.FindAsync(meetIdToDelete);
        Assert.Null(meetInDb);
    }

    [Fact]
    public async Task JoinMeetAsync_ShouldAddUserToMeet_WhenBothExist()
    {
        // Arrange
        var user = await SeedUserAsync();
        var meet = new Domain.Models.Meet(new MeetRequest
        {
            Name = "Joinable Meet",
            Location = "Location",
            Description = "Test Description",
            Coordinates = "47.4979,19.0402",
            Date = DateTime.UtcNow.AddDays(1),
            Tags = new List<MeetTags> { MeetTags.Show }
        }, user, null);
        _dbContext.Meets.Add(meet);

        var anotherUser = new Domain.Models.User { Id = 2, Nickname = "AnotherUser", Email = "another@example.com", PasswordHash = "hash" };
        _dbContext.Users.Add(anotherUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _meetService.JoinMeetAsync(meet.Id, anotherUser.Id);

        // Assert
        Assert.True(result.Success);
        var meetInDb = await _dbContext.Meets.Include(m => m.Users).FirstOrDefaultAsync(m => m.Id == meet.Id);
        Assert.NotNull(meetInDb);
        Assert.Contains(meetInDb.Users, u => u.Id == anotherUser.Id);
    }
}