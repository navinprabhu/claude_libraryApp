using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Shared.Models.DTOs
{
    public class MemberDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public bool IsActive { get; set; }
        public string MembershipNumber { get; set; } = string.Empty;
    }

    public class CreateMemberDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
    }
}