using Xunit;
using Moq;
using FluentAssertions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For InMemory or mocking
using Microsoft.Extensions.Options;   // For IOptions
using ThesisBackend.Application.Authentication.Services;
using ThesisBackend.Application.Authentication.Interfaces;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Data;
using ThesisBackend.Services.Authentication.Interfaces;
using ThesisBackend.Services.Authentication.Models; // For JwtSettings (if ITokenGenerator depends on it directly)

public class AuthServiceTests
{
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ITokenGenerator> _mockTokenGenerator;
    private readonly dbContext _dbContext; // Use InMemory for testing
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockTokenGenerator = new Mock<ITokenGenerator>();

        // Setup InMemory DbContext
        var options = new DbContextOptionsBuilder<dbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid()
                .ToString()) // Unique DB name for each test class instance
            .Options;
        _dbContext = new dbContext(options);
        
        _authService = new AuthService(_dbContext, _mockPasswordHasher.Object, _mockTokenGenerator.Object);
    }
    
    [Fact]
    public async Task RegisterAsync_WithValidNewUser_ShouldReturnSuccessAndAddUserToDatabase()
    {
        // Arrange
        var request = new RegistrationRequest { Email = "test@example.com", Nickname = "tester", Password = "Password123!" };
        var hashedPassword = "hashed_password_value";
        _mockPasswordHasher.Setup(h => h.HashPassword(request.Password)).Returns(hashedPassword);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.UserResponse.Should().NotBeNull();
        result.UserResponse.Email.Should().Be(request.Email);
        result.ErrorMessage.Should().BeNull();

        _mockPasswordHasher.Verify(h => h.HashPassword(request.Password), Times.Once);

        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        userInDb.Should().NotBeNull();
        userInDb.Nickname.Should().Be(request.Nickname);
        userInDb.PasswordHash.Should().Be(hashedPassword);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessAndToken()
    {
        // Arrange
        var password = "Password123!";
        var hashedPassword = "hashed_password_value"; // Assume this is what's stored
        var user = new User { Id = 1, Email = "test@example.com", Nickname = "tester", PasswordHash = hashedPassword };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequest { Email = "test@example.com", Password = password };
        var expectedToken = "generated_jwt_token";

        _mockPasswordHasher.Setup(h => h.VerifyPassword(password, hashedPassword)).Returns(true);
        _mockTokenGenerator.Setup(g => g.GenerateAccessToken(It.Is<User>(u => u.Email == user.Email))).Returns(expectedToken);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Success.Should().BeTrue();
        result.UserResponse.Should().NotBeNull();
        result.UserResponse.Email.Should().Be(user.Email);
        result.Token.Should().Be(expectedToken);
        result.ErrorMessage.Should().BeNull();

        _mockPasswordHasher.Verify(h => h.VerifyPassword(password, hashedPassword), Times.Once);
        _mockTokenGenerator.Verify(g => g.GenerateAccessToken(It.Is<User>(u => u.Email == user.Email)), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "a@a.a", Password = "Password123!" };
        
        // Act
        var result = await _authService.LoginAsync(loginRequest);
        
        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email or password.");
        result.UserResponse.Should().BeNull();
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIncorrect_ShouldReturnFailure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "a@a.a",
            Nickname = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!")
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        var loginRequest = new LoginRequest { Email = "a@a.a", Password = "WrongPassword!" };
        
        _mockPasswordHasher.Setup(h => h.VerifyPassword(loginRequest.Password, user.PasswordHash)).Returns(false);
        
        // Act
        var result = await _authService.LoginAsync(loginRequest);
        
        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email or password.");
        result.UserResponse.Should().BeNull();
        result.Token.Should().BeNull();
    }
    
    [Fact]
    public async Task LoginAsync_WhenEmptyRequest_ShouldReturnFailure()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "", Password = "" };
        
        // Act
        var result = await _authService.LoginAsync(loginRequest);
        
        // Assert
        result.Success.Should().BeFalse();
        result.UserResponse.Should().BeNull();
        result.Token.Should().BeNull();
        result.ErrorMessage.Should().Contain("Invalid email or password.");
    }
}