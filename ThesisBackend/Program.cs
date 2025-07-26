using System.Text;
using Amazon; // For RegionEndpoint
using Amazon.CloudWatchLogs; // For AmazonCloudWatchLogsClient and IAmazonCloudLogs
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch; // For the CloudWatch Sink
using ThesisBackend.Data;
using ThesisBackend.Application.Authentication.Interfaces;
using ThesisBackend.Application.Authentication.Services;
using ThesisBackend.Services.Authentication.Interfaces;
using ThesisBackend.Services.Authentication.Models;
using ThesisBackend.Services.Authentication.Validators;
using Serilog.Debugging;
using ThesisBackend;
using ThesisBackend.Application.UserService.Interfaces;
using ThesisBackend.Services.UserService.Services;
using ThesisBackend.Services.UserService.Validators;

// --- Main application entry point ---
var builder = WebApplication.CreateBuilder(args);

// --- Conditionally configure Serilog based on the builder's environment ---
// This entire block will be skipped in the "Testing" environment, preventing the error.
if (!builder.Environment.IsEnvironment("Testing"))
{
    SelfLog.Enable(Console.Error);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", "RevNRoll-ThesisBackend")
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId();

        // Configure CloudWatch sink
        var cloudWatchConfig = context.Configuration.GetSection("Serilog:CloudWatch");
        var logGroupName = cloudWatchConfig["LogGroupName"];
        var regionSystemName = cloudWatchConfig["Region"];

        if (!string.IsNullOrEmpty(logGroupName) && !string.IsNullOrEmpty(regionSystemName))
        {
            var awsRegion = RegionEndpoint.GetBySystemName(regionSystemName);
            if (awsRegion == null)
            {
                Console.Error.WriteLine($"Invalid AWS Region specified in configuration: {regionSystemName}. CloudWatch logging will be disabled.");
            }
            else
            {
                IAmazonCloudWatchLogs cloudWatchClient = new AmazonCloudWatchLogsClient(awsRegion);
                var cloudWatchSinkOptions = new CloudWatchSinkOptions
                {
                    LogGroupName = logGroupName,
                    TextFormatter = new Serilog.Formatting.Json.JsonFormatter(),
                    MinimumLogEventLevel = Enum.TryParse<LogEventLevel>(cloudWatchConfig["MinimumLogEventLevel"], true, out var level) ? level : LogEventLevel.Information,
                };

                loggerConfiguration.WriteTo.AmazonCloudWatch(cloudWatchSinkOptions, cloudWatchClient);
                Console.WriteLine($"Serilog configured to write to AWS CloudWatch. Log Group: {logGroupName}, Region: {regionSystemName}");
            }
        }
        else
        {
            Console.Error.WriteLine("AWS LogGroupName or Region for CloudWatch sink is not configured. CloudWatch logging will be disabled.");
        }
    });
}


// --- Service Configuration ---
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers()
    .AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssemblyContaining<RegistrationRequestValidator>();
        config.RegisterValidatorsFromAssemblyContaining<UserRequestValidator>();
        config.AutomaticValidationEnabled = false;
    });

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.Configure<ConnetcionString>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserService, UserSerivce>();

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

// --- Database Configuration (skipped in test environment) ---
if (!builder.Environment.IsEnvironment("Testing"))
{
    var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(defaultConnectionString))
    {
        throw new InvalidOperationException("DefaultConnection string is missing or empty in configuration.");
    }
    builder.Services.AddDbContext<dbContext>(options =>
        options.UseNpgsql(defaultConnectionString));
}

// --- Authentication Configuration ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Secret) ||
            string.IsNullOrEmpty(jwtSettings.Issuer) || string.IsNullOrEmpty(jwtSettings.Audience))
        {
            throw new InvalidOperationException("JWT settings (Secret, Issuer, or Audience) are not configured properly.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
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

// --- Middleware Pipeline ---
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseSerilogRequestLogging(options => {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"]);
        };
    });
}

app.UseCors("AllowAngularApp");

if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbCtx = scope.ServiceProvider.GetRequiredService<dbContext>();
        var appLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            appLogger.LogInformation("Attempting to apply database migrations...");
            dbCtx.Database.Migrate();
            appLogger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            appLogger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
}
else
{
    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            var errorLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            if (exceptionHandlerFeature?.Error != null)
            {
                errorLogger.LogError(exceptionHandlerFeature.Error, "UnhandledException: Path={Path} {RequestMethod}", context.Request.Path, context.Request.Method);
            }
            else
            {
                errorLogger.LogError("UnhandledException: An unknown error occurred. Path={Path} {RequestMethod}", context.Request.Path, context.Request.Method);
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected internal server error occurred.",
                Detail = "We are sorry, something went wrong on our end. Please try again later.",
                Instance = context.Request.Path
            };
            await context.Response.WriteAsJsonAsync(problemDetails);
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


// Make Program class partial for WebApplicationFactory in integration tests
public partial class Program { }

namespace ThesisBackend.Services.Authentication.Models
{
    public class ConnetcionString
    {
        public string DefaultConnection { get; set; } = string.Empty;
    }
}
