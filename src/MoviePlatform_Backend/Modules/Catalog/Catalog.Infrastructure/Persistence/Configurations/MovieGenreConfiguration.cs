using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoviePlatform.Modules.Catalog.Domain.Entities;

namespace MoviePlatform.Modules.Catalog.Infrastructure.Persistence.Configurations;

public class MovieGenreConfiguration : IEntityTypeConfiguration<MovieGenre>
{
    public void Configure(EntityTypeBuilder<MovieGenre> builder)
    {
        builder.HasKey(mg => new { mg.MovieId, mg.GenreId });

        builder.HasOne(mg => mg.Movie)
            .WithMany(m => m.MovieGenres)
            .HasForeignKey(mg => mg.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mg => mg.Genre)
            .WithMany()
            .HasForeignKey(mg => mg.GenreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
