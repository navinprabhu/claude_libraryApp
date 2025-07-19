using LibraryApp.AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.AuthService.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                
                entity.Property(e => e.RefreshToken)
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(100);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(100);

                // Indexes
                entity.HasIndex(e => e.Username)
                    .IsUnique();
                
                entity.HasIndex(e => e.Email)
                    .IsUnique();
                
                entity.HasIndex(e => e.RefreshToken);
            });

            // Seed default users
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("password");
            var memberPasswordHash = BCrypt.Net.BCrypt.HashPassword("password");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = adminPasswordHash,
                    Email = "admin@libraryapp.com",
                    Role = UserRoles.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new User
                {
                    Id = 2,
                    Username = "member1",
                    PasswordHash = memberPasswordHash,
                    Email = "member1@libraryapp.com",
                    Role = UserRoles.Member,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            );
        }
    }
}