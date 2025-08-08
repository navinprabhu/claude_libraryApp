using System.ComponentModel.DataAnnotations;

namespace LibraryApp.AuthService.Models
{
    public class TokenValidationRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }

    public class TokenValidationResponse
    {
        public bool IsValid { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<string> Claims { get; set; } = new();
        public DateTime? Expiration { get; set; }
        public string Message { get; set; } = string.Empty;
    }

}