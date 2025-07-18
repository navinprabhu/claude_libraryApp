using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Shared.Models.DTOs
{
    public class BorrowingRecordDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; }
        public decimal? LateFee { get; set; }
        public BookDto Book { get; set; } = new();
        public MemberDto Member { get; set; } = new();
    }

    public class BorrowRequestDto
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        public int MemberId { get; set; }

        public int BorrowDurationDays { get; set; } = 14;
    }
}