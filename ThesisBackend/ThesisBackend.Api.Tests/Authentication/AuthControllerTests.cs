using FluentAssertions;
using System.Net;
using Xunit.Abstractions;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging; // For RemoveAll
using ThesisBackend.Data; // Namespace of your dbContext
using ThesisBackend.Domain.Messages;

namespace ThesisBackend.Api.Tests.Authentication
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output; 

        public AuthControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                // --- Set the environment to "Testing" ---
                builder.UseEnvironment("Testing");

                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // This is the key line to prevent the error
                });
                
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsExtension));
                    services.RemoveAll<DbContextOptions<dbContext>>();
                    services.RemoveAll<dbContext>();

                    var dbName = $"InMemoryDbForTesting_{Guid.NewGuid()}";
                    services.AddDbContext<dbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                    });

                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var appDb = scope.ServiceProvider.GetRequiredService<dbContext>();
                        try
                        {
                            appDb.Database.EnsureCreated();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error ensuring database created in test setup: {ex.Message}");
                            throw; // Rethrow to fail the test setup if DB creation fails
                        }
                    }
                });
            });

            _client = _factory.CreateClient();
            _output = output;
        }

        // ... Your test methods ...
        [Fact]
        public async Task Register_Post_WithValidData_ReturnsOkAndRegistersUser()
        {
            // Arrange
            var request = new RegistrationRequest
            {
                Email = $"validuser@example.com",
                Nickname = $"valid_nickname",
                Password = "ValidPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/Authentication/register", request);

            // Assert
            response.EnsureSuccessStatusCode(); 
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<dbContext>();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                user.Should().NotBeNull();
                user?.Nickname.Should().Be(request.Nickname);
            }
        }
        
        [Fact]
        public async Task Register_Post_WithDuplicateEmail_ReturnsBadRequestProblemDetails()
        {
            // Arrange
            var initialEmail = $"duplicate{Guid.NewGuid().ToString("N").Substring(0, 8)}@example.com";
            var initialNickname = $"initial_nick_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var initialRequest = new RegistrationRequest
            {
                Email = initialEmail,
                Nickname = initialNickname,
                Password = "ValidPassword123!"
            };

            // Register the first user successfully
            var initialResponse = await _client.PostAsJsonAsync("/api/v1/Authentication/register", initialRequest);
            if (!initialResponse.IsSuccessStatusCode) 
            {
                _output.WriteLine($"Initial registration in duplicate test FAILED: {initialResponse.StatusCode}");
                _output.WriteLine(await initialResponse.Content.ReadAsStringAsync());
            }
            initialResponse.EnsureSuccessStatusCode(); 
            
            var duplicateEmailRequest = new RegistrationRequest
            {
                Email = initialEmail, // This is the duplicate email part
                Nickname = $"another_nick_{Guid.NewGuid().ToString("N").Substring(0,8)}",
                Password = "AnotherPassword123!" 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/Authentication/register", duplicateEmailRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Deserialize as ValidationProblemDetails to access the Errors dictionary
            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblemDetails.Should().NotBeNull();

            // 1. Assert the correct title for a FluentValidation failure
            validationProblemDetails?.Title.Should().Be("One or more validation errors occurred.");

            // 2. Assert specific validation errors
            _output.WriteLine($"ValidationProblemDetails for duplicate email request: {await response.Content.ReadAsStringAsync()}");
            
            validationProblemDetails?.Errors.Should().ContainKey("Email"); // Or "Password", "Nickname", etc.
        }
        
        [Fact]
        public async Task Register_Post_WithDuplicateNickname_ReturnsBadRequestProblemDetails()
        {
            // Arrange
            var initialEmail = $"email{Guid.NewGuid().ToString("N").Substring(0, 8)}@example.com";
            var initialNickname = $"duplicate{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var initialRequest = new RegistrationRequest
            {
                Email = initialEmail,
                Nickname = initialNickname,
                Password = "ValidPassword123!"
            };

            // Register the first user successfully
            var initialResponse = await _client.PostAsJsonAsync("/api/v1/Authentication/register", initialRequest);
            if (!initialResponse.IsSuccessStatusCode)
            {
                _output.WriteLine($"Initial registration in duplicate test FAILED: {initialResponse.StatusCode}");
                _output.WriteLine(await initialResponse.Content.ReadAsStringAsync());
            }
            initialResponse.EnsureSuccessStatusCode(); 
            
            var duplicateNicknameRequest = new RegistrationRequest
            {
                Email = $"email{Guid.NewGuid().ToString("N").Substring(0, 8)}@example.com", // This is the duplicate email part
                Nickname = initialNickname,
                Password = "AnotherPassword123!" 
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/Authentication/register", duplicateNicknameRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Deserialize as ValidationProblemDetails to access the Errors dictionary
            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblemDetails.Should().NotBeNull();

            // 1. Assert the correct title for a FluentValidation failure
            validationProblemDetails?.Title.Should().Be("One or more validation errors occurred.");

            // 2. Assert specific validation errors
            _output.WriteLine($"ValidationProblemDetails for duplicate nickname request: {await response.Content.ReadAsStringAsync()}");
            
            validationProblemDetails?.Errors.Should().ContainKey("Nickname");
        }
        
        [Fact]
        public async Task Register_Post_WithLongNickname_ReturnsBadRequestProblemDetails()
        {
            // Arrange
            var initialEmail = $"email{Guid.NewGuid().ToString("N").Substring(0, 8)}@example.com";
            var initialNickname = $"TooVeryLongNicknameThisShouldbeLongEnough{
                Guid.NewGuid().ToString("N")}"; // Exceeding the limit of 32
            var initialRequest = new RegistrationRequest
            {
                Email = initialEmail,
                Nickname = initialNickname,
                Password = "ValidPassword123!"
            };

            // Register the first user successfully
            var response = await _client.PostAsJsonAsync("/api/v1/Authentication/register", initialRequest);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Deserialize as ValidationProblemDetails to access the Errors dictionary
            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblemDetails.Should().NotBeNull();

            // 1. Assert the correct title for a FluentValidation failure
            validationProblemDetails?.Title.Should().Be("One or more validation errors occurred.");

            // 2. Assert specific validation errors
            _output.WriteLine($"ValidationProblemDetails for too long nickname request: {await response.Content.ReadAsStringAsync()}");
            
            validationProblemDetails?.Errors.Should().ContainKey("Nickname");
        }
        
        [Fact]
        public async Task Register_Post_WithPasswordTooShort_ReturnsValidationProblem()
        {
            // Arrange
            var request = new RegistrationRequest
            {
                Email = $"shortpass_{Guid.NewGuid().ToString("N").Substring(0, 8)}@example.com",
                Nickname = $"User_ShortPass_{Guid.NewGuid().ToString("N").Substring(0, 6)}",
                Password = "Short1!" // 7 chars, validator expects min 8
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/Authentication/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblemDetails.Should().NotBeNull();
            validationProblemDetails?.Title.Should().Be("One or more validation errors occurred.");
            validationProblemDetails?.Errors.Should().ContainKey("Password");
            validationProblemDetails?.Errors["Password"].Should().Contain("Password must be at least 8 characters long.");
            _output.WriteLine($"ValidationProblemDetails for short password: {await response.Content.ReadAsStringAsync()}");
        }
        
        [Fact]
        public async Task Register_Post_WithPasswordMissingSpecialChar_ReturnsValidationProblem()
        {
            // Arrange
            var request = new RegistrationRequest
            {
                Email = $"nospecial_{Guid.NewGuid().ToString("N").Substring(0, 8)}@example.com",
                Nickname = $"User_NoSpecial_{Guid.NewGuid().ToString("N").Substring(0, 6)}",
                Password = "NoSpecial123" // Missing special character
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/Authentication/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblemDetails.Should().NotBeNull();
            validationProblemDetails?.Title.Should().Be("One or more validation errors occurred.");
            validationProblemDetails?.Errors.Should().ContainKey("Password");
            validationProblemDetails?.Errors["Password"].Should().Contain("Password must contain at least one special character.");
            _output.WriteLine($"ValidationProblemDetails for missing special char: {await response.Content.ReadAsStringAsync()}");
        }
        
        //Login tests
        
        [Fact]
        public async Task Login_Post_WithValidCredentials_ReturnsOkAndUserDataWithCookie()
        {
            // Arrange
            var userEmail = $"logintest_{Guid.NewGuid().ToString("N").Substring(0,8)}@example.com";
            var userNickname = $"LoginUser_{Guid.NewGuid().ToString("N").Substring(0,6)}";
            var userPassword = "ValidPassword123!"; // Ensure this passes RegistrationValidator rules

            // 1. Register the user first (or seed directly if preferred, ensuring password is hashed)
            var registrationRequest = new RegistrationRequest
            {
                Email = userEmail,
                Nickname = userNickname,
                Password = userPassword
            };
            var registerResponse = await _client.PostAsJsonAsync("/api/v1/Authentication/register", registrationRequest);
            _output.WriteLine("Initial registration successful for login test.");
            if (!registerResponse.IsSuccessStatusCode)
            {
                _output.WriteLine($"Login test setup: Registration failed! {await registerResponse.Content.ReadAsStringAsync()}");
            }
            registerResponse.EnsureSuccessStatusCode();

            var loginRequest = new LoginRequest
            {
                Email = userEmail,
                Password = userPassword
            };
            
            // Act
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/Authentication/login", loginRequest);
            _output.WriteLine($"Login attempt status code: {loginResponse.StatusCode}");
            var loginResponseBody = await loginResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"Login attempt response body: {loginResponseBody}");

            // Assert
            loginResponse.EnsureSuccessStatusCode();
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await loginResponse.Content.ReadFromJsonAsync<UserResponse>();
            responseData.Should().NotBeNull();
            responseData?.Nickname.Should().Be(userNickname);

            var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
            cookies.Should().ContainSingle(cookie => cookie.StartsWith("accessToken="));
            var accessTokenCookie = cookies.First(cookie => cookie.StartsWith("accessToken="));
            accessTokenCookie.Should().Contain("httponly");
            accessTokenCookie.Should().Contain("samesite=lax");
            // You can add more checks for Secure, Path, and Expires if needed.
            _output.WriteLine($"Login successful. User: {responseData?.Nickname}, Cookie: {accessTokenCookie}");
        }
        
        [Fact]
        public async Task Login_Post_WithNonExistentEmail_ReturnsValidationProblem()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = $"nonexistent_{Guid.NewGuid().ToString("N").Substring(0,8)}@example.com",
                Password = "SomePassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/Authentication/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest); 
            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblemDetails.Should().NotBeNull();
            validationProblemDetails?.Title.Should().Be("One or more validation errors occurred.");
            validationProblemDetails?.Errors.Should().ContainKey("Email");
            validationProblemDetails?.Errors["Email"].Should().Contain("An account does not exist with this email address.");
            _output.WriteLine($"ValidationProblemDetails for non-existent email: {await response.Content.ReadAsStringAsync()}");
        }
        
        [Fact]
        public async Task Login_Post_WithIncorrectPassword_ReturnsUnauthorizedProblemDetails()
        {
            // Arrange
            var userEmail = $"wrongpass_{Guid.NewGuid().ToString("N").Substring(0,8)}@example.com";
            var userNickname = $"WrongPassUser_{Guid.NewGuid().ToString("N").Substring(0,6)}";
            var correctPassword = "ValidPassword123!";
            var incorrectPassword = "WrongPassword123!";

            // 1. Register the user
            var registrationRequest = new RegistrationRequest
            {
                Email = userEmail,
                Nickname = userNickname,
                Password = correctPassword
            };
            var registerResponse = await _client.PostAsJsonAsync("/api/v1/Authentication/register", registrationRequest);
            if (!registerResponse.IsSuccessStatusCode)
            {
                _output.WriteLine($"Login test (wrong pass) setup: Registration failed! {await registerResponse.Content.ReadAsStringAsync()}");
            }
            registerResponse.EnsureSuccessStatusCode();

            var loginRequest = new LoginRequest
            {
                Email = userEmail,
                Password = incorrectPassword
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/Authentication/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // Expecting 401 from AuthService failure
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails?.Title.Should().Be("Login Failed");
            problemDetails?.Detail.Should().BeOneOf("Invalid email or password."); 
            _output.WriteLine($"ProblemDetails for incorrect password: {await response.Content.ReadAsStringAsync()}");
        }
        
        [Fact]
        public async Task Login_Post_WithPasswordTooShort_ReturnsValidationProblem()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = $"anyemail_{Guid.NewGuid().ToString("N").Substring(0,8)}@example.com", 
                Password = "Short" // Less than 8 characters
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/Authentication/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validationProblemDetails.Should().NotBeNull();
            validationProblemDetails?.Title.Should().Be("One or more validation errors occurred.");
            validationProblemDetails?.Errors.Should().ContainKey("Password");
            validationProblemDetails?.Errors["Password"].Should().Contain("Password must be at least 8 characters long."); 
            _output.WriteLine($"ValidationProblemDetails for short login password: {await response.Content.ReadAsStringAsync()}");
        }
    }
}
