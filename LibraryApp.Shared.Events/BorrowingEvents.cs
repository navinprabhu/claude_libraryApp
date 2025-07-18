namespace LibraryApp.Shared.Events
{
    public class BookBorrowedEvent : BaseEvent
    {
        public int BorrowingRecordId { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;
    }

    public class BookReturnedEvent : BaseEvent
    {
        public int BorrowingRecordId { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime ReturnDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsLate { get; set; }
        public decimal? LateFee { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;
    }

    public class BookOverdueEvent : BaseEvent
    {
        public int BorrowingRecordId { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;
    }
}