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
using ThesisBackend.Services.Authentication.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssemblyContaining<RegistrationRequestValidator>();
        config.RegisterValidatorsFromAssemblyContaining<LoginRequestValidator>();
        config.AutomaticValidationEnabled = false;
    });
builder.Services.AddOpenApi(); // Add OpenAPI/Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.Configure<ConnetcionString>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>(); // Assuming this is also needed by AuthService or other services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>(); // Assuming this is also needed

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // If needed
    });
});
// Configure the database context with PostgreSQL
builder.Services.AddDbContext<dbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
        };

        // Add custom logic to extract the token from the cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Extract the token from the 'accessToken' cookie
                var token = context.Request.Cookies["accessToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token; // Set the token for validation
                }
                return Task.CompletedTask;
            }
        };
    });
var app = builder.Build();
app.UseCors("AllowAngularApp");
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<dbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        dbContext.Database.Migrate(); // Apply migrations
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw; // Re-throw the exception to stop the application
    }
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage(); // Use developer exception page in development
}
else
{
    app.Run(async context =>
    {
        var exceptionHandler = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (exceptionHandler != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exceptionHandler.Error, "An unhandled exception has occurred.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError; // Internal Server Error
            context.Response.ContentType = "application/json";
            
            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Title = "An unexpected internal server error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "We are sorry, something went wrong on our end. Please try again later.",
                Instance = context.Request.Path
            };
        }
    });
    app.UseHsts();
}

app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Map OpenAPI endpoints in development
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS

app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization(); // Enable authorization middleware

app.MapControllers(); // Map controller routes

app.Run(); // Run the application