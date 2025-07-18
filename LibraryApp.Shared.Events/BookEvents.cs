namespace LibraryApp.Shared.Events
{
    public abstract class BaseEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string CorrelationId { get; set; } = string.Empty;
    }

    public class BookCreatedEvent : BaseEvent
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
    }

    public class BookUpdatedEvent : BaseEvent
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
    }

    public class BookDeletedEvent : BaseEvent
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}