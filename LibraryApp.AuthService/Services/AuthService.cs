using LibraryApp.AuthService.Data;
using LibraryApp.AuthService.Models;
using LibraryApp.Shared.Models.Common;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace LibraryApp.AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            JwtTokenService jwtTokenService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for username: {Username}", request.Username);

                // Find user by username
                var user = await _userRepository.GetByUsernameAsync(request.Username);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
                    return ApiResponse<LoginResponse>.ErrorResponse("Invalid username or password", 401);
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: User is inactive - {Username}", request.Username);
                    return ApiResponse<LoginResponse>.ErrorResponse("Account is disabled", 401);
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid password - {Username}", request.Username);
                    return ApiResponse<LoginResponse>.ErrorResponse("Invalid username or password", 401);
                }

                // Generate token response
                var tokenResponse = _jwtTokenService.GenerateTokenResponse(user);

                // Update user with refresh token
                user.RefreshToken = tokenResponse.RefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days from now
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Login successful for user: {Username}", request.Username);
                return ApiResponse<LoginResponse>.SuccessResponse(tokenResponse, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
                return ApiResponse<LoginResponse>.ErrorResponse("An error occurred during login", 500);
            }
        }

        public async Task<ApiResponse<TokenValidationResponse>> ValidateTokenAsync(string token)
        {
            try
            {
                var principal = _jwtTokenService.ValidateToken(token);
                if (principal == null)
                {
                    return ApiResponse<TokenValidationResponse>.ErrorResponse("Invalid token", 401);
                }

                var username = principal.FindFirst(ClaimTypes.Name)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                var expClaim = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
                
                DateTime? expiration = null;
                if (long.TryParse(expClaim, out var exp))
                {
                    expiration = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                }

                var claims = principal.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();

                var response = new TokenValidationResponse
                {
                    IsValid = true,
                    Username = username ?? string.Empty,
                    Role = role ?? string.Empty,
                    Claims = claims,
                    Expiration = expiration,
                    Message = "Token is valid"
                };

                return ApiResponse<TokenValidationResponse>.SuccessResponse(response, "Token validated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return ApiResponse<TokenValidationResponse>.ErrorResponse("Token validation failed", 500);
            }
        }

        public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                // Find user by refresh token
                var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
                if (user == null)
                {
                    _logger.LogWarning("Refresh token not found or expired");
                    return ApiResponse<LoginResponse>.ErrorResponse("Invalid refresh token", 401);
                }

                // Validate the original token (even if expired)
                var principal = _jwtTokenService.ValidateToken(request.Token);
                if (principal == null)
                {
                    _logger.LogWarning("Invalid token provided for refresh");
                    return ApiResponse<LoginResponse>.ErrorResponse("Invalid token", 401);
                }

                // Verify the token belongs to the same user
                var tokenUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (tokenUserId != user.Id.ToString())
                {
                    _logger.LogWarning("Token user ID mismatch during refresh");
                    return ApiResponse<LoginResponse>.ErrorResponse("Token mismatch", 401);
                }

                // Generate new token response
                var tokenResponse = _jwtTokenService.GenerateTokenResponse(user);

                // Update user with new refresh token
                user.RefreshToken = tokenResponse.RefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Token refreshed successfully for user: {Username}", user.Username);
                return ApiResponse<LoginResponse>.SuccessResponse(tokenResponse, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return ApiResponse<LoginResponse>.ErrorResponse("An error occurred while refreshing token", 500);
            }
        }

        public async Task<ApiResponse<object>> GetUserInfoAsync(string token)
        {
            try
            {
                var principal = _jwtTokenService.ValidateToken(token);
                if (principal == null)
                {
                    return ApiResponse<object>.ErrorResponse("Invalid token", 401);
                }

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt))
                {
                    return ApiResponse<object>.ErrorResponse("Invalid user ID in token", 401);
                }

                var user = await _userRepository.GetByIdAsync(userIdInt);
                if (user == null)
                {
                    return ApiResponse<object>.ErrorResponse("User not found", 404);
                }

                var userInfo = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Role,
                    user.IsActive,
                    user.CreatedAt,
                    user.LastLoginAt,
                    Claims = principal.Claims.Select(c => new { c.Type, c.Value }).ToList()
                };

                return ApiResponse<object>.SuccessResponse(userInfo, "User information retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info");
                return ApiResponse<object>.ErrorResponse("An error occurred while retrieving user information", 500);
            }
        }

        public async Task<ApiResponse<User>> RegisterUserAsync(string username, string email, string password, string role = "Member")
        {
            try
            {
                // Check if username exists
                if (await _userRepository.UsernameExistsAsync(username))
                {
                    return ApiResponse<User>.ErrorResponse("Username already exists", 400);
                }

                // Check if email exists
                if (await _userRepository.EmailExistsAsync(email))
                {
                    return ApiResponse<User>.ErrorResponse("Email already exists", 400);
                }

                // Validate role
                if (!UserRoles.AllRoles.Contains(role))
                {
                    return ApiResponse<User>.ErrorResponse("Invalid role", 400);
                }

                // Create new user
                var user = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                var createdUser = await _userRepository.CreateAsync(user);
                _logger.LogInformation("User registered successfully: {Username}", username);

                return ApiResponse<User>.SuccessResponse(createdUser, "User registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Username}", username);
                return ApiResponse<User>.ErrorResponse("An error occurred during registration", 500);
            }
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return false;

                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
                return false;
            }
        }
    }
}