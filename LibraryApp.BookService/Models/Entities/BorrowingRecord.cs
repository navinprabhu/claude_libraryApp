using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryApp.BookService.Models.Entities
{
    public class BorrowingRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        [StringLength(100)]
        public string MemberName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string MemberEmail { get; set; } = string.Empty;

        [Required]
        public DateTime BorrowedAt { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? ReturnedAt { get; set; }

        public bool IsReturned { get; set; } = false;

        public bool IsOverdue => !IsReturned && DateTime.UtcNow > DueDate;

        [StringLength(500)]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? LateFee { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        [ForeignKey(nameof(BookId))]
        public virtual Book Book { get; set; } = null!;
    }
}