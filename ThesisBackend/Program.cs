using System.Text;
using Amazon; // For RegionEndpoint
using Amazon.CloudWatchLogs; // For AmazonCloudWatchLogsClient and IAmazonCloudWatchLogs
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch; // For the CloudWatch Sink
using ThesisBackend.Data;
// Assuming your services and validators are in these namespaces or similar
using ThesisBackend.Application.Authentication.Interfaces;
using ThesisBackend.Application.Authentication.Services;
using ThesisBackend.Services.Authentication.Interfaces;
using ThesisBackend.Services.Authentication.Models;
// If JwtSettings is in a specific models/options folder:
// using ThesisBackend.Models.Configuration; // Example if you have JwtSettings class here
using ThesisBackend.Services.Authentication.Validators; // For RegistrationRequestValidator

// --- Bootstrap Serilog for early logging (as you had) ---
var configurationForBootstrap = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configurationForBootstrap)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try // Added try-finally for robust Serilog flushing
{
    Log.Information("Application Starting Up...");

    var builder = WebApplication.CreateBuilder(args);

    // --- Configure Serilog for the application host ---
    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration) // Reads "Serilog" section from appsettings.json
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", "RevNRoll-ThesisBackend")
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId();
            // Console sink can be configured via ReadFrom.Configuration if specified in appsettings.json

        // Configure CloudWatch sink
        var cloudWatchConfig = context.Configuration.GetSection("Serilog:CloudWatch");
        var logGroupName = cloudWatchConfig["LogGroupName"];
        var regionSystemName = cloudWatchConfig["Region"];

        if (!string.IsNullOrEmpty(logGroupName) && !string.IsNullOrEmpty(regionSystemName))
        {
            var awsRegion = RegionEndpoint.GetBySystemName(regionSystemName);
            if (awsRegion == null)
            {
                // Log with the bootstrap logger if host logger isn't fully up or use Console.Error
                Log.Warning("Invalid AWS Region specified in configuration: {ConfiguredRegion}. CloudWatch logging will be disabled.", regionSystemName);
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
                Log.Information("Serilog configured to write to AWS CloudWatch. Log Group: {LogGroup}, Region: {AWSRegion}", logGroupName, regionSystemName);
            }
        }
        else
        {
            Log.Warning("AWS LogGroupName or Region for CloudWatch sink is not configured in appsettings.json (Serilog:CloudWatch). CloudWatch logging will be disabled.");
        }
    });

    bool isTestEnvironment = builder.Environment.IsEnvironment("Testing");

    builder.Services.AddHttpContextAccessor(); // For SerilogRequestLogging

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

    // Your existing Configure line for ConnetcionString. Ensure 'ConnetcionString' class is defined if used this way.
    // If it's a typo and not used for options, it could be removed. Otherwise, ensure the class exists.
    // For DbContext, GetConnectionString("DefaultConnection") is used directly below.
    builder.Services.Configure<ConnetcionString>(builder.Configuration.GetSection("ConnectionStrings"));

    // Configure JwtSettings using the class you provided at the end of your previous snippet
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

    if (!isTestEnvironment)
    {
        var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(defaultConnectionString))
        {
            Log.Fatal("DefaultConnection string is missing or empty in configuration."); // Use bootstrap logger
            throw new InvalidOperationException("DefaultConnection string is missing or empty in configuration.");
        }
        builder.Services.AddDbContext<dbContext>(options =>
            options.UseNpgsql(defaultConnectionString));
    }

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Fetch strongly-typed settings.
            // Ensure JwtSettings class is accessible here (e.g., via using statement or fully qualified name)
            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<ThesisBackend.Services.Authentication.Models.JwtSettings>();

            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Secret) ||
                string.IsNullOrEmpty(jwtSettings.Issuer) || string.IsNullOrEmpty(jwtSettings.Audience))
            {
                Log.Fatal("JWT settings (Secret, Issuer, or Audience) are not configured properly."); // Use bootstrap logger
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

    // --- Add Serilog Request Logging ---
    // This should be early in the pipeline, after routing if you use routing data, but before Auth.
    app.UseSerilogRequestLogging(options => {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"]);
            // Optionally, add user claim if available AFTER UseAuthentication runs
            // var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if(!string.IsNullOrEmpty(userId)) diagnosticContext.Set("UserId", userId);
        };
    });

    app.UseCors("AllowAngularApp");

    if (!isTestEnvironment)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbCtx = scope.ServiceProvider.GetRequiredService<dbContext>();
            // Use ILogger from DI, which will be Serilog's logger
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
                throw; // Re-throw to halt startup if migration fails
            }
        }
    }

    if (app.Environment.IsDevelopment() || isTestEnvironment)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage(); // Good for both dev and your "Testing" env
        if (app.Environment.IsDevelopment()) // Only map OpenApi in true development
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
                // Use ILogger from DI for consistent logging
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

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application host terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class partial for WebApplicationFactory in integration tests
public partial class Program { }

// It's better to place these class definitions in their own files,
// for example, in a "Configuration" or "Models" folder.
// namespace ThesisBackend.Models.Configuration // Or ThesisBackend.Services.Authentication.Models
namespace ThesisBackend.Services.Authentication.Models // Matching your using statement
{
    // If this class is used by builder.Services.Configure<ConnetcionString>
    // ensure the name matches exactly, including casing.
    // If "ConnetcionString" was a typo for a general ConnectionStrings section,
    // you might not need this specific class if you only use GetConnectionString("DefaultConnection").
    public class ConnetcionString // Spelling as per your provided code.
    {
        public string DefaultConnection { get; set; } = string.Empty;
    }
}