using LibraryApp.MemberService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.MemberService.Data
{
    public class MemberDbContext : DbContext
    {
        public MemberDbContext(DbContextOptions<MemberDbContext> options) : base(options)
        {
        }

        public DbSet<Member> Members { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.MembershipType).HasMaxLength(50);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.JoinedDate).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.MaxBooksAllowed).IsRequired();

                entity.HasIndex(e => new { e.FirstName, e.LastName });
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsActive);
            });

            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>().HasData(
                new Member
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "+1-555-0123",
                    Address = "123 Main St, Anytown, USA",
                    DateOfBirth = new DateTime(1985, 6, 15),
                    JoinedDate = DateTime.UtcNow.AddMonths(-6),
                    Status = MemberStatus.Active,
                    MembershipType = "Premium",
                    MaxBooksAllowed = 10,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    IsActive = true
                },
                new Member
                {
                    Id = 2,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    PhoneNumber = "+1-555-0124",
                    Address = "456 Oak Ave, Another City, USA",
                    DateOfBirth = new DateTime(1990, 3, 22),
                    JoinedDate = DateTime.UtcNow.AddMonths(-3),
                    Status = MemberStatus.Active,
                    MembershipType = "Standard",
                    MaxBooksAllowed = 5,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    IsActive = true
                },
                new Member
                {
                    Id = 3,
                    FirstName = "Bob",
                    LastName = "Johnson",
                    Email = "bob.johnson@example.com",
                    PhoneNumber = "+1-555-0125",
                    Address = "789 Pine Rd, Third Town, USA",
                    DateOfBirth = new DateTime(1978, 11, 8),
                    JoinedDate = DateTime.UtcNow.AddMonths(-12),
                    Status = MemberStatus.Suspended,
                    MembershipType = "Standard",
                    MaxBooksAllowed = 5,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    IsActive = true
                }
            );
        }
    }
}