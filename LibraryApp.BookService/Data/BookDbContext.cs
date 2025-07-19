using LibraryApp.BookService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.BookService.Data
{
    public class BookDbContext : DbContext
    {
        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowingRecord> BorrowingRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ISBN).IsUnique();
                entity.Property(e => e.ISBN).IsRequired().HasMaxLength(13);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Genre).HasMaxLength(100);
                entity.Property(e => e.Publisher).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();

                entity.HasMany(e => e.BorrowingRecords)
                      .WithOne(e => e.Book)
                      .HasForeignKey(e => e.BookId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BorrowingRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MemberName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MemberEmail).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.LateFee).HasColumnType("decimal(10,2)");
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.BorrowedAt).IsRequired();
                entity.Property(e => e.DueDate).IsRequired();

                entity.HasIndex(e => new { e.BookId, e.MemberId, e.BorrowedAt });
                entity.HasIndex(e => e.DueDate);
                entity.HasIndex(e => e.IsReturned);
            });

            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    ISBN = "9780134685991",
                    Title = "Effective Java",
                    Author = "Joshua Bloch",
                    Genre = "Programming",
                    Publisher = "Addison-Wesley",
                    PublishedYear = 2017,
                    Description = "The definitive guide to Java programming language best practices.",
                    TotalCopies = 5,
                    AvailableCopies = 5,
                    Status = LibraryApp.Shared.Models.Enums.BookStatus.Available,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    IsActive = true
                },
                new Book
                {
                    Id = 2,
                    ISBN = "9780135957059",
                    Title = "The Pragmatic Programmer",
                    Author = "David Thomas, Andrew Hunt",
                    Genre = "Programming",
                    Publisher = "Addison-Wesley",
                    PublishedYear = 2019,
                    Description = "Your journey to mastery in software development.",
                    TotalCopies = 3,
                    AvailableCopies = 3,
                    Status = LibraryApp.Shared.Models.Enums.BookStatus.Available,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    IsActive = true
                },
                new Book
                {
                    Id = 3,
                    ISBN = "9780132350884",
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    Genre = "Programming",
                    Publisher = "Prentice Hall",
                    PublishedYear = 2008,
                    Description = "A handbook of agile software craftsmanship.",
                    TotalCopies = 4,
                    AvailableCopies = 2,
                    Status = LibraryApp.Shared.Models.Enums.BookStatus.Available,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    IsActive = true
                }
            );
        }
    }
}