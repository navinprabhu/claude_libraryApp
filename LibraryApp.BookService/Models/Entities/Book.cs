using System.ComponentModel.DataAnnotations;
using LibraryApp.Shared.Models.Enums;

namespace LibraryApp.BookService.Models.Entities
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(13)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Author { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Genre { get; set; }

        [StringLength(100)]
        public string? Publisher { get; set; }

        public int? PublishedYear { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int TotalCopies { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableCopies { get; set; }

        public BookStatus Status { get; set; } = BookStatus.Available;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<BorrowingRecord> BorrowingRecords { get; set; } = new List<BorrowingRecord>();
    }
}