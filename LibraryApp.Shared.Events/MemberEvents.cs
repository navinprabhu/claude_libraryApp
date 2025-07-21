namespace LibraryApp.Shared.Events
{
    public class MemberRegisteredEvent : BaseEvent
    {
        public int MemberId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MembershipNumber { get; set; } = string.Empty;
    }

    public class MemberUpdatedEvent : BaseEvent
    {
        public int MemberId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class MemberDeactivatedEvent : BaseEvent
    {
        public int MemberId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime DeactivationDate { get; set; }
    }

    public class MemberStatusChangedEvent : BaseEvent
    {
        public int MemberId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime StatusChangeDate { get; set; }
        public string? Reason { get; set; }
    }
}