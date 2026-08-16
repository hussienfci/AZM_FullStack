using Microsoft.EntityFrameworkCore;
using UserManagementApi.Models;

namespace UserManagementApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Movie> Movies { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Genre> Genres { get; set; } = null!;
    public DbSet<Watchlist> Watchlists { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.ToTable("Users");
        });

        // Movies
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).ValueGeneratedOnAdd();
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Description).HasMaxLength(2000);
            entity.Property(m => m.Genre).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Director).HasMaxLength(150);
            entity.Property(m => m.PosterUrl).HasMaxLength(500);
            entity.Property(m => m.TrailerUrl).HasMaxLength(500);
            entity.Property(m => m.Language).HasMaxLength(50);
            entity.Property(m => m.Country).HasMaxLength(100);
            entity.HasIndex(m => m.Title).HasDatabaseName("IX_Movies_Title");
            entity.HasIndex(m => m.Genre).HasDatabaseName("IX_Movies_Genre");
            entity.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(m => m.IsActive).HasDefaultValue(true);
            entity.ToTable("Movies");
        });

        // Reviews
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedOnAdd();
            entity.Property(r => r.Rating).IsRequired();
            entity.Property(r => r.Comment).HasMaxLength(2000);
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(r => r.IsActive).HasDefaultValue(true);

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Movie)
                .WithMany()
                .HasForeignKey(r => r.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => r.UserId).HasDatabaseName("IX_Reviews_UserId");
            entity.HasIndex(r => r.MovieId).HasDatabaseName("IX_Reviews_MovieId");
            entity.HasIndex(r => new { r.UserId, r.MovieId }).IsUnique().HasDatabaseName("IX_Reviews_UserId_MovieId");

            entity.ToTable("Reviews");
        });


        // ── Genres ──
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Id).ValueGeneratedOnAdd();
            entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(g => g.Name).IsUnique();
            entity.Property(g => g.Description).HasMaxLength(500);
            entity.Property(g => g.IsActive).HasDefaultValue(true);
            entity.Property(g => g.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.ToTable("Genres");
        });
        

        // ── Watchlists ──
        modelBuilder.Entity<Watchlist>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Id).ValueGeneratedOnAdd();
            entity.Property(w => w.IsWatched).HasDefaultValue(false);
            entity.Property(w => w.IsActive).HasDefaultValue(true);
            entity.Property(w => w.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(w => w.Movie)
                .WithMany()
                .HasForeignKey(w => w.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(w => w.UserId).HasDatabaseName("IX_Watchlists_UserId");
            entity.HasIndex(w => w.MovieId).HasDatabaseName("IX_Watchlists_MovieId");
            entity.HasIndex(w => new { w.UserId, w.MovieId }).IsUnique().HasDatabaseName("IX_Watchlists_UserId_MovieId");

            entity.ToTable("Watchlists");
        });
        

        base.OnModelCreating(modelBuilder);
    }
}