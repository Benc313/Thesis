using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ThesisBackend.Application.UserService.Interfaces;
using ThesisBackend.Controllers;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Services.UserService.Services;

namespace ThesisBackend.Services.Tests.User.Services;

public class UserServiceTest
{
    private readonly UserSerivce _userService;
    private readonly Mock<ILogger<UserSerivce>> _mockLogger;
    private readonly dbContext _dbContext; // Use InMemory for testing

    public UserServiceTest()
    {
        // Setup InMemory DbContext
        var options = new DbContextOptionsBuilder<dbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid()
                .ToString()) // Unique DB name for each test class instance
            .Options;
        _dbContext = new dbContext(options);

        _mockLogger = new Mock<ILogger<UserSerivce>>();
        _userService = new UserSerivce(_dbContext, _mockLogger.Object);
    }
    
    [Fact]
    public async Task GetUser_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var testUser = new Domain.Models.User { Id = 1, Nickname = "TestUser", Email = "test@example.com", PasswordHash = "hashed_password" };
        _dbContext.Users.Add(testUser);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var result = await _userService.GetUser(1);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.UserResponse);
        Assert.Equal(1, int.Parse(result.UserResponse.Id));
        Assert.Equal("TestUser", result.UserResponse.Nickname);
    }
    
    [Fact]
    public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange (No user is added to the database)

        // Act
        var result = await _userService.GetUser(99); // Use an ID that won't exist

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.UserResponse);
        Assert.Equal("User not found", result.ErrorMessage);
    }
    
    [Fact]
    public async Task UpdateUser_ShouldUpdateUser_WhenUserExists()
    {
        string email = "original@example.com";
        // Arrange
        var originalUser = new Domain.Models.User { Id = 1, Nickname = "OriginalNick", Email = email, Description = "Original Desc", PasswordHash = "hashed_password" };
        _dbContext.Users.Add(originalUser);
        await _dbContext.SaveChangesAsync();

        var updateUserRequest = new UserRequest
        {
            Nickname = "UpdatedNick",
            Email = email,
            Description = "Updated Desc"
        };

        // Act
        var result = await _userService.UpdateUser(updateUserRequest, 1);
        var updatedUser = await _dbContext.Users.FindAsync(1);


        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.UserResponse);
        Assert.Equal("UpdatedNick", result.UserResponse.Nickname);
        Assert.Equal("UpdatedNick", updatedUser.Nickname);
        Assert.Equal("Updated Desc", updatedUser.Description);
    }
    
    [Fact]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var updateUserRequest = new UserRequest
        {
            Nickname = "AnyNick",
            Email = "any@example.com",
            Description = "Any Desc"
        };

        // Act
        var result = await _userService.UpdateUser(updateUserRequest, 99); // Non-existent user

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found", result.ErrorMessage);
    }
    
    [Fact]
    public async Task GetUsers_ShouldReturnAllUsers_WhenUsersExist()
    {
        // Arrange
        _dbContext.Users.AddRange(
            new Domain.Models.User { Id = 1, Nickname = "UserOne", Email = "one@example.com", PasswordHash = "hashed_password" },
            new Domain.Models.User { Id = 2, Nickname = "UserTwo", Email = "two@example.com", PasswordHash = "hashed_password" }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userService.GetUsers();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.UserResponse);
        Assert.Equal(2, result.UserResponse.Count());
    }

}