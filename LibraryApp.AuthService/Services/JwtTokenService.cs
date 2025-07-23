using LibraryApp.AuthService.Configuration;
using LibraryApp.AuthService.Models;
using LibraryApp.Shared.Infrastructure.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryApp.AuthService.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(IOptions<JwtSettings> jwtSettings, ILogger<JwtTokenService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public string GenerateToken(IEnumerable<Claim> claims)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation("JWT token generated successfully for user");
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token");
                throw;
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = _jwtSettings.ValidateIssuer,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = _jwtSettings.ValidateAudience,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = _jwtSettings.ValidateLifetime,
                    ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes)
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                return jwtToken.ValidTo <= DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking token expiration");
                return true;
            }
        }

        public string RefreshToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal == null)
                {
                    throw new SecurityTokenException("Invalid token");
                }

                var newClaims = principal.Claims.Where(c => c.Type != JwtRegisteredClaimNames.Exp &&
                                                          c.Type != JwtRegisteredClaimNames.Iat &&
                                                          c.Type != JwtRegisteredClaimNames.Nbf);

                return GenerateToken(newClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                throw;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public LoginResponse GenerateTokenResponse(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add additional claims based on role
            var permissions = GetPermissionsForRole(user.Role);
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var token = GenerateToken(claims);
            var refreshToken = GenerateRefreshToken();
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            return new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = expiration,
                ExpiresIn = _jwtSettings.ExpirationMinutes * 60, // Convert to seconds
                User = new UserProfile
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    LastLoginAt = user.LastLoginAt,
                    IsActive = user.IsActive
                },
                Permissions = permissions
            };
        }

        private static List<string> GetPermissionsForRole(string role)
        {
            return role switch
            {
                UserRoles.Admin => new List<string>
                {
                    // Full dashboard access
                    "dashboard:view", "dashboard:stats", "dashboard:reports",
                    // Books management
                    "books:read", "books:write", "books:delete", "books:categories", "books:statistics",
                    // Members management  
                    "members:read", "members:write", "members:delete", "members:statistics", "members:active",
                    // Borrowing management
                    "borrowing:read", "borrowing:write", "borrowing:delete", "borrowing:statistics", "borrowing:recent",
                    // System administration
                    "system:admin", "system:reports", "system:alerts"
                },
                UserRoles.Member => new List<string>
                {
                    // Limited dashboard access
                    "dashboard:view-personal",
                    // Books browsing
                    "books:read", "books:search", "books:availability",
                    // Own profile management
                    "members:read-own", "members:write-own",
                    // Own borrowing management
                    "borrowing:read-own", "borrowing:write-own", "borrowing:history-own"
                },
                _ => new List<string>()
            };
        }
    }
}