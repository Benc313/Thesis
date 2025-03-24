using Microsoft.EntityFrameworkCore;
using ThesisBackend.Models;

namespace ThesisBackend.Data;

public class dbContext : DbContext
{
    public DbSet<Car> Cars { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Crew> Crews { get; set; }
    public DbSet<UserCrew> UserCrews { get; set; }
    public DbSet<Race> Races { get; set; }
    public DbSet<Meet> Meets { get; set; }

    public dbContext(DbContextOptions<dbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Nickname)
            .IsUnique();

        modelBuilder.Entity<Crew>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // Configure UserCrew relationships (explicit junction table)
        modelBuilder.Entity<UserCrew>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserCrews)
            .HasForeignKey(uc => uc.UserId);

        modelBuilder.Entity<UserCrew>()
            .HasOne(uc => uc.Crew)
            .WithMany(c => c.UserCrews)
            .HasForeignKey(uc => uc.CrewId);

        // Configure many-to-many relationship between User and Race
        modelBuilder.Entity<User>()
            .HasMany(u => u.Races)
            .WithMany(r => r.Users)
            .UsingEntity(j => j.ToTable("UserRace"));

        // Configure many-to-many relationship between User and Meet
        modelBuilder.Entity<User>()
            .HasMany(u => u.Meets)
            .WithMany(m => m.Users)
            .UsingEntity(j => j.ToTable("UserMeet"));

        // Race relationships
        modelBuilder.Entity<Race>()
            .HasOne(r => r.Creator)
            .WithMany()
            .HasForeignKey(r => r.CreatorId);

        modelBuilder.Entity<Race>()
            .HasOne(r => r.Crew)
            .WithMany()
            .HasForeignKey(r => r.CrewId)
            .OnDelete(DeleteBehavior.SetNull);

        // Meet relationships
        modelBuilder.Entity<Meet>()
            .HasOne(m => m.Creator)
            .WithMany()
            .HasForeignKey(m => m.CreatorId);

        modelBuilder.Entity<Meet>()
            .HasOne(m => m.Crew)
            .WithMany(c => c.Meets)
            .HasForeignKey(m => m.CrewId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}