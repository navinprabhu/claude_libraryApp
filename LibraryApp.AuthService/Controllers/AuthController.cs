using LibraryApp.AuthService.Models;
using LibraryApp.AuthService.Services;
using LibraryApp.Shared.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LibraryApp.AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user and return JWT token
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 401)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 400)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var errorResponse = ApiResponse<LoginResponse>.ErrorResponse("Validation failed", 400, errors);
                return BadRequest(errorResponse);
            }

            var result = await _authService.LoginAsync(request);
            
            return result.StatusCode switch
            {
                200 => Ok(result),
                401 => Unauthorized(result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <param name="request">Token validation request</param>
        /// <returns>Token validation result</returns>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ApiResponse<TokenValidationResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TokenValidationResponse>), 401)]
        [ProducesResponseType(typeof(ApiResponse<TokenValidationResponse>), 400)]
        public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var errorResponse = ApiResponse<TokenValidationResponse>.ErrorResponse("Validation failed", 400, errors);
                return BadRequest(errorResponse);
            }

            var result = await _authService.ValidateTokenAsync(request.Token);
            
            return result.StatusCode switch
            {
                200 => Ok(result),
                401 => Unauthorized(result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        /// <summary>
        /// Refresh expired JWT token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New JWT token</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 401)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 400)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var errorResponse = ApiResponse<LoginResponse>.ErrorResponse("Validation failed", 400, errors);
                return BadRequest(errorResponse);
            }

            var result = await _authService.RefreshTokenAsync(request);
            
            return result.StatusCode switch
            {
                200 => Ok(result),
                401 => Unauthorized(result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        /// <summary>
        /// Get user information from JWT token
        /// </summary>
        /// <returns>User information and claims</returns>
        [HttpGet("userinfo")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetUserInfo()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                var errorResponse = ApiResponse<object>.ErrorResponse("Missing or invalid authorization header", 401);
                return Unauthorized(errorResponse);
            }

            var token = authHeader["Bearer ".Length..].Trim();
            var result = await _authService.GetUserInfoAsync(token);
            
            return result.StatusCode switch
            {
                200 => Ok(result),
                401 => Unauthorized(result),
                404 => NotFound(result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        /// <summary>
        /// Register a new user (Admin only)
        /// </summary>
        /// <param name="request">User registration details</param>
        /// <returns>Created user information</returns>
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<User>), 201)]
        [ProducesResponseType(typeof(ApiResponse<User>), 400)]
        [ProducesResponseType(typeof(ApiResponse<User>), 401)]
        [ProducesResponseType(typeof(ApiResponse<User>), 403)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var errorResponse = ApiResponse<User>.ErrorResponse("Validation failed", 400, errors);
                return BadRequest(errorResponse);
            }

            var result = await _authService.RegisterUserAsync(request.Username, request.Email, request.Password, request.Role);
            
            return result.StatusCode switch
            {
                200 => CreatedAtAction(nameof(GetUserInfo), result),
                400 => BadRequest(result),
                _ => StatusCode(result.StatusCode, result)
            };
        }

        /// <summary>
        /// Get user permissions based on role
        /// </summary>
        /// <returns>User permissions list</returns>
        [HttpGet("permissions")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        public IActionResult GetUserPermissions()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
            var permissions = User.FindAll("permission").Select(c => c.Value).ToList();
            
            var permissionInfo = new
            {
                role = role,
                permissions = permissions,
                hasAdminAccess = role == UserRoles.Admin,
                hasDashboardAccess = permissions.Contains("dashboard:view") || permissions.Contains("dashboard:view-personal")
            };

            return Ok(ApiResponse<object>.SuccessResponse(permissionInfo, "Permissions retrieved successfully"));
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>Service health status</returns>
        [HttpGet("health")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                service = "LibraryApp.AuthService",
                version = "1.0.0"
            });
        }
    }

    public class RegisterRequest
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = UserRoles.Member;
    }
}