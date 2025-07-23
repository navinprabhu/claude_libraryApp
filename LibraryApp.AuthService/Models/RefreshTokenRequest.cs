using System.ComponentModel.DataAnnotations;

namespace LibraryApp.AuthService.Models
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}