using System.Security.Claims;

namespace LibraryApp.Shared.Infrastructure.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(IEnumerable<Claim> claims);
        ClaimsPrincipal? ValidateToken(string token);
        bool IsTokenExpired(string token);
        string RefreshToken(string token);
    }
}