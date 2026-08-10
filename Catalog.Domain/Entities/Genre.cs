using MoviePlatform.Shared.Kernel.Entities;

namespace MoviePlatform.Modules.Catalog.Domain.Entities;

public class Genre : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Genre() { }

    public static Genre Create(string name, string? description = null)
    {
        return new Genre
        {
            Name = name,
            Description = description
        };
    }

    public void Update(string name, string? description = null)
    {
        Name = name;
        Description = description;
        UpdateTimestamp();
    }
}
