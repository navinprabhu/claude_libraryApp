using AspNetCoreRateLimit;
using LibraryApp.ApiGateway.Extensions;
using LibraryApp.ApiGateway.Middleware;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog logging
builder.Services.AddSerilogLogging(builder.Configuration);
builder.Host.UseSerilog();

// Add Ocelot configuration
builder.Configuration.AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"Configuration/ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add services to the container
builder.Services.AddApiGatewayServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Security middleware
app.UseHttpsRedirection();
app.UseHsts();

// Custom middleware pipeline
app.UseCorrelationId();
app.UseRequestLogging();

// CORS
app.UseCors("ApiGatewayPolicy");

// Rate limiting
app.UseIpRateLimiting();
app.UseCustomRateLimiting();

// Authentication
app.UseAuthentication();
app.UseAuthorization();

// Health checks endpoint
app.MapHealthChecks("/health");
app.MapControllers();

// Add root endpoint with API information
app.MapGet("/", async (HttpContext context) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    
    var apiInfo = new
    {
        name = "LibraryApp API Gateway",
        version = "1.0.0",
        environment = app.Environment.EnvironmentName,
        timestamp = DateTime.UtcNow,
        correlationId = correlationId,
        endpoints = new
        {
            health = new
            {
                gateway = "/health/gateway",
                services = "/health/services",
                individual = new { auth = "/health/auth", books = "/health/books", members = "/health/members" }
            },
            api = new
            {
                auth = "/api/auth/*",
                books = "/api/books/*", 
                members = "/api/members/*"
            },
            documentation = new
            {
                message = "Visit individual service Swagger UIs:",
                auth = "http://localhost:5001",
                books = "http://localhost:5002",
                members = "http://localhost:5003"
            }
        }
    };
    
    Log.Information("API Gateway root endpoint accessed with correlation ID: {CorrelationId}", correlationId);
    
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(apiInfo, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
});

// Add aggregated health check endpoint
app.MapGet("/health/gateway", async (HttpContext context) =>
{
    var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    
    var healthInfo = new
    {
        service = "ApiGateway",
        status = "Healthy",
        timestamp = DateTime.UtcNow,
        correlationId = correlationId,
        version = "1.0.0",
        environment = app.Environment.EnvironmentName
    };
    
    Log.Information("Health check requested for API Gateway with correlation ID: {CorrelationId}", correlationId);
    
    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(healthInfo));
});

// Global exception handling
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
        Log.Error(ex, "Unhandled exception occurred for correlation ID: {CorrelationId}", correlationId);
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = "Internal Server Error",
            message = "An unexpected error occurred",
            correlationId = correlationId,
            timestamp = DateTime.UtcNow
        };
        
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// Use Ocelot
await app.UseOcelot();

Log.Information("API Gateway started successfully on {Environment}", app.Environment.EnvironmentName);

app.Run();

public partial class Program { }
