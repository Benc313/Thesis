using Microsoft.EntityFrameworkCore;
using ThesisBackend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Add controllers for handling HTTP requests
builder.Services.AddOpenApi(); // Add OpenAPI/Swagger for API documentation

// Configure the database context with PostgreSQL
builder.Services.AddDbContext<dbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Apply pending migrations and create the database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<dbContext>();
    dbContext.Database.Migrate(); // Apply migrations
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Map OpenAPI endpoints in development
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS

app.UseAuthorization(); // Enable authorization middleware

app.MapControllers(); // Map controller routes

app.Run(); // Run the application