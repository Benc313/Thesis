using Xunit;
using Moq;
using FluentAssertions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For InMemory or mocking
using Microsoft.Extensions.Options;   // For IOptions
using ThesisBackend.Application.Authentication.Services;
using ThesisBackend.Application.Authentication.Interfaces;
using ThesisBackend.Application.Authentication.Models;
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
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var existingEmail = "existing@example.com";
        var existingUser = new User
        {
            // Id will be set by InMemory DB if not specified, or you can set it
            Email = existingEmail,
            Nickname = "ExistingUser",
            PasswordHash = "some_hashed_password" // Password hash needed for valid User entity
        };
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(); // Save the existing user to the in-memory database

        var request = new RegistrationRequest
        {
            Email = existingEmail, // Attempt to register with the same email
            Nickname = "NewUserNickname",
            Password = "NewPassword123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Email already exists.");
        result.UserResponse.Should().BeNull();
        result.Token.Should().BeNull();

        // Verify that no new user was added with this email (or any user with the new nickname)
        var userCountWithEmail = await _dbContext.Users.CountAsync(u => u.Email == existingEmail);
        userCountWithEmail.Should().Be(1); // Should still be only the one we seeded

        var userWithNewNickname = await _dbContext.Users.FirstOrDefaultAsync(u => u.Nickname == request.Nickname);
        userWithNewNickname.Should().BeNull(); // No user with the new nickname should have been created
    }

    [Fact]
    public async Task RegisterAsync_WhenNicknameAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var existingNickname = "existingNickname";
        var existingUser = new User
        {
            Email = "validemail1@mail.com",
            Nickname = existingNickname,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ValidPassword123!")
        };
        _dbContext.Users.Add(existingUser);
        _dbContext.SaveChangesAsync();

        var request = new RegistrationRequest
        {
            Email = "validemail2@mail.com",
            Nickname = existingNickname, // Attempt to register with the same nickname
            Password = "AnotherPassword123!"
        };
        
        // Act
        var result = await _authService.RegisterAsync(request);
        
        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Nickname already exists.");
        result.UserResponse.Should().BeNull();
        result.Token.Should().BeNull();
        
        // Verify that no new user was added with this nickname
        var userCountWithNickname = await _dbContext.Users.CountAsync(u => u.Nickname == existingNickname);
        userCountWithNickname.Should().Be(1); // Should still be only the one we seeded
        
        var userWithNewEmail = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        userWithNewEmail.Should().BeNull(); // No user with the new email should have been created
    }
    
    [Fact]
    public async Task RegisterAsync_WhenInvalidRequest_ShouldReturnFailure()
    {
        var request = new RegistrationRequest
        {
            Email = "invalid-email", // Invalid email format
            Nickname = "", // Empty nickname
            Password = "short" // Too short password
        };
        
        // Act
        var result = await _authService.RegisterAsync(request);
        
        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.UserResponse.Should().BeNull();
        result.Token.Should().BeNull();
        
        // Verify that no user was added to the database
        var userCount = await _dbContext.Users.CountAsync();
        userCount.Should().Be(0); // Should be zero since the registration should fail
        
        result.ErrorMessage.Should().Contain("Invalid registration request.");
        result.ErrorMessage.Should().Contain("Invalid email address.");
        result.ErrorMessage.Should().Contain("Nickname cannot be empty.");
        result.ErrorMessage.Should().Contain("Password must be at least 8 characters long.");
        result.ErrorMessage.Should().Contain("Password must contain at least one digit.");
        result.ErrorMessage.Should().Contain("Password must contain at least one uppercase letter.");
        result.ErrorMessage.Should().Contain("Password must contain at least one special character");
    }
    
    [Fact]
    public async Task RegisterAsync_WhenEmptyRequest_ShouldReturnFailure()
    {
        // Arrange
        var request = new RegistrationRequest { Email = "", Nickname = "", Password = "" };
        
        // Act
        var result = await _authService.RegisterAsync(request);
        
        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Email is required.");
        result.ErrorMessage.Should().Contain("Nickname is required.");
        result.ErrorMessage.Should().Contain("Password is required.");
        result.UserResponse.Should().BeNull();
        result.Token.Should().BeNull();
        
        // Verify that no user was added to the database
        var userCount = await _dbContext.Users.CountAsync();
        userCount.Should().Be(0); // Should be zero since the registration should fail
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
        result.ErrorMessage.Should().Contain("A valid email address is required.");
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
    public async Task LoginAsync_WhenInvalidRequest_ShouldReturnSuccessAndToken()
    {
        var loginRequest = new LoginRequest { Email = "invalidEmail", Password = "short" };
        
        // Act
        var result = await _authService.LoginAsync(loginRequest);
        
        // Assert
        result.Success.Should().BeFalse();
        result.UserResponse.Should().BeNull();
        result.Token.Should().BeNull();
        result.ErrorMessage.Should().Contain("A valid email address is required.");
        result.ErrorMessage.Should().Contain("Password must be at least 8 characters long.");
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
        result.ErrorMessage.Should().Contain("Email is required.");
        result.ErrorMessage.Should().Contain("Password is required.");
    }
}