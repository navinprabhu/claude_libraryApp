using LibraryApp.BookService.Data;
using LibraryApp.BookService.Data.Repositories;
using LibraryApp.BookService.Infrastructure.Authorization;
using LibraryApp.BookService.Infrastructure.Health;
using LibraryApp.BookService.Infrastructure.Logging;
using LibraryApp.BookService.Infrastructure.Mapping;
using LibraryApp.BookService.Infrastructure.Middleware;
using LibraryApp.BookService.Services;
using LibraryApp.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
LoggingConfiguration.ConfigureSerilog(builder.Configuration, builder.Environment);
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Configure Entity Framework with In-Memory Database
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseInMemoryDatabase("BookServiceDb"));

// Add repositories
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBorrowingRepository, BorrowingRepository>();

// Add business services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowingService, BorrowingService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<BookServiceHealthCheck>("bookservice")
    .AddCheck<DatabaseHealthCheck>("database");

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LibraryApp Book Service API",
        Version = "v1",
        Description = "Book management service for the Library Management System",
        Contact = new OpenApiContact
        {
            Name = "Library App Team",
            Email = "support@libraryapp.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add shared infrastructure services (event publishing, correlation ID)
builder.Services.AddSharedInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseGlobalExceptionHandling();

// Add shared middleware (correlation ID, request logging)
app.UseSharedMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LibraryApp Book Service API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
}

app.UseCors();

app.UseJwtAuthentication();

app.UseAuthorization();

app.MapControllers();

// Add health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// Initialize database with sample data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    await context.Database.EnsureCreatedAsync();
}

Log.Information("LibraryApp.BookService starting up...");

app.Run();
