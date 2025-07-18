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
}