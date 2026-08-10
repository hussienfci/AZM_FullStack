using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoviePlatform.Modules.Catalog.Domain.Entities;

namespace MoviePlatform.Modules.Catalog.Infrastructure.Persistence.Configurations;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(g => g.Description)
            .HasMaxLength(200);

        builder.HasIndex(g => g.Name)
            .IsUnique();

        builder.HasQueryFilter(g => !g.IsDeleted);
    }
}
