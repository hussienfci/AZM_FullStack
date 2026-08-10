using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoviePlatform.Modules.Catalog.Domain.Entities;

namespace MoviePlatform.Modules.Catalog.Infrastructure.Persistence.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.PosterUrl)
            .HasMaxLength(500);

        builder.Property(m => m.BackdropUrl)
            .HasMaxLength(500);

        builder.Property(m => m.TrailerUrl)
            .HasMaxLength(500);

        builder.Property(m => m.Director)
            .HasMaxLength(100);

        builder.Property(m => m.Language)
            .HasMaxLength(50);

        builder.Property(m => m.AverageRating)
            .HasPrecision(3, 2);

        builder.HasIndex(m => m.Title);
        builder.HasIndex(m => m.ReleaseYear);
        builder.HasIndex(m => m.IsFeatured);
        builder.HasIndex(m => m.AverageRating);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
