using System.ComponentModel.DataAnnotations;

namespace LibraryApp.MemberService.Models.Entities
{
    public class Member
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public MemberStatus Status { get; set; } = MemberStatus.Active;

        [StringLength(50)]
        public string MembershipType { get; set; } = "Standard";

        public int MaxBooksAllowed { get; set; } = 5;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }

    public enum MemberStatus
    {
        Active = 0,
        Suspended = 1,
        Inactive = 2,
        Pending = 3
    }
}