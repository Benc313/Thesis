using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;

namespace ThesisBackend.Services.Tests.Crew;

public class CrewServiceTest
{
    private readonly CrewService.Services.CrewService _crewService;
    private readonly dbContext _dbContext;
    private readonly Mock<ILogger<CrewService.Services.CrewService>> _mockLogger;

    public CrewServiceTest()
    {
        var options = new DbContextOptionsBuilder<dbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new dbContext(options);
        _mockLogger = new Mock<ILogger<CrewService.Services.CrewService>>();
        _crewService = new CrewService.Services.CrewService(_dbContext, _mockLogger.Object);
    }

    private async Task<Domain.Models.User> SeedUserAsync(int id = 1, string nickname = "TestUser")
    {
        var user = new Domain.Models.User { Id = id, Nickname = nickname, Email = $"{nickname}@test.com", PasswordHash = "hash" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task CreateCrewAsync_ShouldCreateCrew_AndAssignUserAsLeader()
    {
        var user = await SeedUserAsync();
        var request = new CrewRequest { Name = "Test Crew", Description = "A cool crew.", ImageLocation = "image.jpg" };

        var result = await _crewService.CreateCrewAsync(request, user.Id);

        result.Success.Should().BeTrue();
        result.Crew.Should().NotBeNull();
        result.Crew!.Name.Should().Be("Test Crew");

        var crewInDb = await _dbContext.Crews.Include(c => c.UserCrews).FirstOrDefaultAsync();
        crewInDb.Should().NotBeNull();
        crewInDb!.UserCrews.Should().ContainSingle(uc => uc.UserId == user.Id && uc.Rank == Rank.Leader);
    }

    [Fact]
    public async Task AddUserToCrewAsync_ShouldAddUserWithMemberRank()
    {
        var leader = await SeedUserAsync(1, "Leader");
        var newUser = await SeedUserAsync(2, "NewMember");
        var crewResult = await _crewService.CreateCrewAsync(new CrewRequest
        {
            Name = "The Crew", Description = "A cool crew.", ImageLocation = "image.jpg"
        }, leader.Id);
        
        var addUserRequest = new UserCrewRequest { UserId = newUser.Id, Rank = Rank.Member };

        var result = await _crewService.AddUserToCrewAsync(addUserRequest, crewResult.Crew!.Id);

        result.Success.Should().BeTrue();
        var crewInDb = await _dbContext.Crews.Include(c => c.UserCrews).FirstAsync();
        crewInDb.UserCrews.Should().HaveCount(2);
        crewInDb.UserCrews.Should().ContainSingle(uc => uc.UserId == newUser.Id && uc.Rank == Rank.Member);
    }

}