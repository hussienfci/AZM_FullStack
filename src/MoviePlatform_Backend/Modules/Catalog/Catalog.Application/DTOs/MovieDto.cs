using System;
using System.Collections.Generic;

namespace MoviePlatform.Modules.Catalog.Application.DTOs;

public class MovieSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public int ReleaseYear { get; set; }
    public decimal? AverageRating { get; set; }
    public int? DurationMinutes { get; set; }
    public List<string> Genres { get; set; } = new();
    public bool IsFeatured { get; set; }
}

public class MovieDetailsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public int ReleaseYear { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Director { get; set; }
    public string? Language { get; set; }
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsFeatured { get; set; }
    public List<GenreDto> Genres { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class GenreDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
