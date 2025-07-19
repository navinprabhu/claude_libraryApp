using System.ComponentModel.DataAnnotations;
using LibraryApp.MemberService.Models.Entities;

namespace LibraryApp.MemberService.Models.Requests
{
    public class UpdateMemberRequest
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public MemberStatus? Status { get; set; }

        [StringLength(50)]
        public string? MembershipType { get; set; }

        public int? MaxBooksAllowed { get; set; }
    }

    public class CreateMemberRequest
    {
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

        [Required]
        public DateTime DateOfBirth { get; set; }

        [StringLength(50)]
        public string MembershipType { get; set; } = "Standard";

        [Range(1, 20)]
        public int MaxBooksAllowed { get; set; } = 5;
    }

    public class MemberStatusUpdateRequest
    {
        [Required]
        public MemberStatus Status { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}