using LibraryApp.Shared.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Shared.Models.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public BookStatus Status { get; set; }
        public DateTime PublishedDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
    }

    public class CreateBookDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        public DateTime PublishedDate { get; set; }

        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int TotalCopies { get; set; }
    }

    public class UpdateBookDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(100)]
        public string? Author { get; set; }

        [StringLength(20)]
        public string? ISBN { get; set; }

        public DateTime? PublishedDate { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int? TotalCopies { get; set; }
    }
}