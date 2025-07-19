using LibraryApp.AuthService.Models;
using LibraryApp.Shared.Models.Common;

namespace LibraryApp.AuthService.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<TokenValidationResponse>> ValidateTokenAsync(string token);
        Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<ApiResponse<object>> GetUserInfoAsync(string token);
        Task<ApiResponse<User>> RegisterUserAsync(string username, string email, string password, string role = "Member");
        Task<bool> UpdateLastLoginAsync(int userId);
    }
}