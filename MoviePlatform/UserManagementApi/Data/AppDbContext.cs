// using Microsoft.EntityFrameworkCore;
// using UserManagementApi.Models;

// namespace UserManagementApi.Data;

// public class AppDbContext : DbContext
// {
//     public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

//     public DbSet<User> Users { get; set; }

//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//         modelBuilder.Entity<User>(entity =>
//         {
//             entity.HasKey(u => u.Id);
//             entity.Property(u => u.Id).ValueGeneratedOnAdd();
//             entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
//             entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
//             entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
//             entity.HasIndex(u => u.Email).IsUnique();
//             entity.Property(u => u.PasswordHash).IsRequired();
//             entity.Property(u => u.PhoneNumber).HasMaxLength(20);
//             entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);
//             entity.Property(u => u.IsActive).HasDefaultValue(true);
//             entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
//             entity.ToTable("Users");
//         });

//         base.OnModelCreating(modelBuilder);
//     }
// }


using Microsoft.EntityFrameworkCore;
using UserManagementApi.Models;

namespace UserManagementApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        base.OnModelCreating(modelBuilder);
    }
}