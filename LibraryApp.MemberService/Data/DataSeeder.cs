using LibraryApp.MemberService.Models.Entities;

namespace LibraryApp.MemberService.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(MemberDbContext context)
        {
            if (context.Members.Any())
            {
                return; // Database already seeded
            }

            var members = new List<Member>
            {
                new Member
                {
                    FirstName = "Alice",
                    LastName = "Williams",
                    Email = "alice.williams@example.com",
                    PhoneNumber = "+1-555-0126",
                    Address = "321 Elm St, Demo City, USA",
                    DateOfBirth = new DateTime(1992, 8, 14),
                    JoinedDate = DateTime.UtcNow.AddMonths(-8),
                    Status = MemberStatus.Active,
                    MembershipType = "Premium",
                    MaxBooksAllowed = 10,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "DataSeeder",
                    IsActive = true
                },
                new Member
                {
                    FirstName = "Charlie",
                    LastName = "Brown",
                    Email = "charlie.brown@example.com",
                    PhoneNumber = "+1-555-0127",
                    Address = "654 Maple Dr, Sample Town, USA",
                    DateOfBirth = new DateTime(1988, 12, 3),
                    JoinedDate = DateTime.UtcNow.AddMonths(-4),
                    Status = MemberStatus.Active,
                    MembershipType = "Standard",
                    MaxBooksAllowed = 5,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "DataSeeder",
                    IsActive = true
                },
                new Member
                {
                    FirstName = "Diana",
                    LastName = "Davis",
                    Email = "diana.davis@example.com",
                    PhoneNumber = "+1-555-0128",
                    Address = "987 Cedar Ln, Test Village, USA",
                    DateOfBirth = new DateTime(1995, 4, 27),
                    JoinedDate = DateTime.UtcNow.AddMonths(-2),
                    Status = MemberStatus.Pending,
                    MembershipType = "Standard",
                    MaxBooksAllowed = 5,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "DataSeeder",
                    IsActive = true
                }
            };

            context.Members.AddRange(members);
            await context.SaveChangesAsync();
        }
    }
}