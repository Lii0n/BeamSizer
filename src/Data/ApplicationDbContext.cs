// BeamCalculator.Data/Models/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore; // Fixed: Add this using statement
using System.ComponentModel.DataAnnotations;

namespace BeamCalculator.Data.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Database tables
        public DbSet<User> Users { get; set; }
        public DbSet<SavedAnalysis> SavedAnalyses { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure SavedAnalysis entity
            modelBuilder.Entity<SavedAnalysis>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProjectName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.LastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // JSON column for storing analysis results (PostgreSQL specific)
                entity.Property(e => e.AnalysisResultsJson).HasColumnType("jsonb");
                entity.Property(e => e.ConfigurationJson).HasColumnType("jsonb");

                // Relationships
                entity.HasOne(s => s.User)
                      .WithMany(u => u.SavedAnalyses)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ProjectName);
                entity.HasIndex(e => e.CreatedDate);
            });

            // Configure Project entity
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relationships
                entity.HasOne(p => p.User)
                      .WithMany(u => u.Projects)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.SavedAnalyses)
                      .WithOne(s => s.Project)
                      .HasForeignKey(s => s.ProjectId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed initial data
            SeedInitialData(modelBuilder);
        }

        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // Seed default admin user with STATIC datetime and STATIC password hash
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@alexandercases.com",
                    // Replace the BCrypt call with a pre-generated hash
                    PasswordHash = "$2a$11$6IlgUZ8/DFyQOE2GXOyN6OV6rVdl2qQeLiDv8j3oXyFbU7t4fzujC", // "Admin123!"
                    Role = "Admin",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Static date
                    IsActive = true
                }
            );
        }
    }

    // Entity models (keeping your existing models exactly as they are)
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // User, Admin
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<SavedAnalysis> SavedAnalyses { get; set; } = new List<SavedAnalysis>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }

    public class SavedAnalysis
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int? ProjectId { get; set; } // Optional - can be null for standalone analyses
        public string ConfigurationJson { get; set; } = string.Empty; // BeamSizerConfig as JSON
        public string AnalysisResultsJson { get; set; } = string.Empty; // BeamSizingResults as JSON
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public User User { get; set; } = null!;
        public Project? Project { get; set; }
    }

    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Status { get; set; } = "Active"; // Active, Completed, On Hold
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<SavedAnalysis> SavedAnalyses { get; set; } = new List<SavedAnalysis>();
    }
}