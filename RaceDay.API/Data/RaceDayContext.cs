
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(DbContextOptions<RaceDayContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Registration> Registrations { get; set; } = null!;
        public DbSet<Result> Results { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role
            modelBuilder.Entity<Role>()
                .HasKey(r => r.RoleID);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne<Role>()
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.Restrict);

            // Event
            modelBuilder.Entity<Event>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .ToTable("Event", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Event_EventType",
                        "[EventType] IN ('Run', 'Walk', 'Cycle')");
                    table.HasCheckConstraint(
                        "CK_Event_Distance",
                        "[Distance] >= 0");
                });

            // Category
            modelBuilder.Entity<Category>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.EventID, c.CategoryName })
                .IsUnique();

            // Registration
            modelBuilder.Entity<Registration>()
                .ToTable("Registration", table =>
                    table.HasCheckConstraint(
                        "CK_Registration_Status",
                        "[Status] IN ('Pending', 'Confirmed', 'Cancelled')"));

            modelBuilder.Entity<Registration>()
                .Property(r => r.Status)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Registration>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Registration>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(r => r.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Registration>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(r => r.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Registration>()
                .HasIndex(r => new
                {
                    r.UserID,
                    r.EventID,
                    r.CategoryID
                })
                .IsUnique();

            // Result
            modelBuilder.Entity<Result>()
                .HasOne<Registration>()
                .WithMany()
                .HasForeignKey(r => r.RegistrationID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Result>()
                .HasIndex(r => r.RegistrationID)
                .IsUnique();

            modelBuilder.Entity<Result>()
                .ToTable("Result", table =>
                    table.HasCheckConstraint(
                        "CK_Result_Position",
                        "[Position] > 0"));
        }
    }
}
