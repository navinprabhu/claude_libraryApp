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
