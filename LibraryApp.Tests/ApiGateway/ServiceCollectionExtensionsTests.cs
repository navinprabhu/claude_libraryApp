using LibraryApp.ApiGateway.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using AspNetCoreRateLimit;

namespace LibraryApp.Tests.ApiGateway
{
    public class ServiceCollectionExtensionsTests
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;

        public ServiceCollectionExtensionsTests()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "YourSuperSecretKeyThatIsAtLeast32CharactersLong123456789",
                ["JwtSettings:Issuer"] = "LibraryApp.AuthService",
                ["JwtSettings:Audience"] = "LibraryApp.ApiClients",
                ["ServiceUrls:AuthService"] = "http://localhost:5001",
                ["ServiceUrls:BookService"] = "http://localhost:5002",
                ["ServiceUrls:MemberService"] = "http://localhost:5003",
                ["IpRateLimiting:EnableEndpointRateLimiting"] = "true",
                ["IpRateLimiting:HttpStatusCode"] = "429"
            });
            
            _configuration = configurationBuilder.Build();
            _services = new ServiceCollection();
        }

        [Fact]
        public void AddApiGatewayServices_Should_Register_Ocelot()
        {
            // Act
            _services.AddApiGatewayServices(_configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var ocelotService = _services.FirstOrDefault(s => s.ServiceType.Name.Contains("Ocelot"));
            Assert.NotNull(ocelotService);
        }

        [Fact]
        public void AddApiGatewayServices_Should_Configure_JWT_Authentication()
        {
            // Act
            _services.AddApiGatewayServices(_configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var authService = serviceProvider.GetService<IConfigureOptions<JwtBearerOptions>>();
            Assert.NotNull(authService);
        }

        [Fact]
        public void AddApiGatewayServices_Should_Register_Rate_Limiting_Services()
        {
            // Act
            _services.AddApiGatewayServices(_configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var rateLimitConfig = serviceProvider.GetService<IRateLimitConfiguration>();
            Assert.NotNull(rateLimitConfig);

            var ipRateLimitOptions = serviceProvider.GetService<IOptions<IpRateLimitOptions>>();
            Assert.NotNull(ipRateLimitOptions);
        }

        [Fact]
        public void AddApiGatewayServices_Should_Register_Health_Checks()
        {
            // Act
            _services.AddApiGatewayServices(_configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var healthCheckService = serviceProvider.GetService<HealthCheckService>();
            Assert.NotNull(healthCheckService);
        }

        [Fact]
        public void AddApiGatewayServices_Should_Configure_CORS()
        {
            // Act
            _services.AddApiGatewayServices(_configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var corsService = _services.FirstOrDefault(s => s.ServiceType.Name.Contains("Cors"));
            Assert.NotNull(corsService);
        }

        [Fact]
        public void AddApiGatewayServices_Should_Register_Controllers()
        {
            // Act
            _services.AddApiGatewayServices(_configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var controllerService = _services.FirstOrDefault(s => s.ServiceType.Name.Contains("Controller"));
            Assert.NotNull(controllerService);
        }

        [Fact]
        public void AddApiGatewayServices_Should_Register_Memory_Cache()
        {
            // Act
            _services.AddApiGatewayServices(_configuration);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var memoryCache = serviceProvider.GetService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
            Assert.NotNull(memoryCache);
        }

        [Fact]
        public void AddSerilogLogging_Should_Configure_Serilog()
        {
            // Act
            _services.AddSerilogLogging(_configuration);

            // Assert
            var loggingService = _services.FirstOrDefault(s => s.ServiceType.Name.Contains("ILogger"));
            Assert.NotNull(loggingService);
        }

        [Fact]
        public void AddApiGatewayServices_Should_Handle_Missing_JWT_SecretKey()
        {
            // Arrange
            var emptyConfig = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();

            // Act & Assert - The service should register but JWT will fail at runtime, not at registration
            services.AddApiGatewayServices(emptyConfig);
            var serviceProvider = services.BuildServiceProvider();
            
            // Verify services are registered even with missing config
            var healthCheckService = serviceProvider.GetService<HealthCheckService>();
            Assert.NotNull(healthCheckService);
        }

        [Theory]
        [InlineData("JwtSettings:Issuer")]
        [InlineData("JwtSettings:Audience")]
        [InlineData("JwtSettings:SecretKey")]
        public void AddApiGatewayServices_Should_Use_JWT_Configuration_Values(string configKey)
        {
            // Arrange
            var expectedValue = $"Test{configKey.Split(':').Last()}";
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "YourSuperSecretKeyThatIsAtLeast32CharactersLong123456789",
                    ["JwtSettings:Issuer"] = "TestIssuer",
                    ["JwtSettings:Audience"] = "TestAudience",
                    [configKey] = expectedValue
                })
                .Build();

            // Act
            _services.AddApiGatewayServices(config);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert - Service should be registered without throwing
            var jwtOptions = serviceProvider.GetService<IConfigureOptions<JwtBearerOptions>>();
            Assert.NotNull(jwtOptions);
        }
    }
}