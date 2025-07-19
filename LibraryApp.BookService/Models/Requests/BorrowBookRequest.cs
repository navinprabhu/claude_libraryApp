using System.ComponentModel.DataAnnotations;

namespace LibraryApp.BookService.Models.Requests
{
    public class BorrowBookRequest
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        [StringLength(100)]
        public string MemberName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string MemberEmail { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class ReturnBookRequest
    {
        [Required]
        public int BorrowingRecordId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? LateFee { get; set; }
    }

    public class ExtendBorrowingRequest
    {
        [Required]
        public int BorrowingRecordId { get; set; }

        [Required]
        public DateTime NewDueDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}