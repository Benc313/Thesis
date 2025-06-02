// In Program.cs

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ThesisBackend;
using ThesisBackend.Data;
using ThesisBackend.Services.Authentication.Models;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using ThesisBackend.Application.Authentication.Interfaces;
using ThesisBackend.Application.Authentication.Services;
using ThesisBackend.Services.Authentication.Interfaces;
using ThesisBackend.Services.Authentication.Models;
using ThesisBackend.Services.Authentication.Validators;

var builder = WebApplication.CreateBuilder(args);

// --- Add this check ---
bool isTestEnvironment = builder.Environment.IsEnvironment("Testing");

// Add services to the container.
builder.Services.AddControllers()
    .AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssemblyContaining<RegistrationRequestValidator>();
        config.AutomaticValidationEnabled = false;
    });

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.Configure<ConnetcionString>(builder.Configuration.GetSection("ConnectionStrings")); // Ensure ConnetcionString is the correct class name
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// --- Conditional DbContext Registration ---
if (!isTestEnvironment)
{
    builder.Services.AddDbContext<dbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}
// In your tests, you will add the InMemory DbContext via ConfigureServices

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? string.Empty))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["accessToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

app.UseCors("AllowAngularApp");

// --- Conditional Migration ---
if (!isTestEnvironment) // Only run migrations if not in a test environment
{
    using (var scope = app.Services.CreateScope())
    {
        var dbCtx = scope.ServiceProvider.GetRequiredService<dbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            dbCtx.Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
}

if (app.Environment.IsDevelopment() || isTestEnvironment) // Also enable Swagger for "Testing" env if desired
{
    app.UseSwagger();
    app.UseSwaggerUI();
    if (isTestEnvironment) // For tests, developer exception page might be useful
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
         app.UseDeveloperExceptionPage(); // Assuming this was intended for non-test dev
    }
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandlerFeature != null)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(exceptionHandlerFeature.Error, "An unhandled exception has occurred.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                // ... rest of your error handling
                 var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected internal server error occurred.",
                    Detail = "We are sorry, something went wrong on our end. Please try again later.",
                    Instance = context.Request.Path
                };
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        });
    });
    app.UseHsts();
}

app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }