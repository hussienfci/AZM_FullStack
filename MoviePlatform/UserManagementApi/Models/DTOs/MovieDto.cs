namespace UserManagementApi.Models.DTOs;

public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Genre { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Director { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public decimal? Rating { get; set; }
    public string? Language { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMovieDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Genre { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Director { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public decimal? Rating { get; set; }
    public string? Language { get; set; }
    public string? Country { get; set; }
}

public class UpdateMovieDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Genre { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Director { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public decimal? Rating { get; set; }
    public string? Language { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
}

public class MovieResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public MovieDto? Data { get; set; }
}

public class MoviesListResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<MovieDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}

public class MovieSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Genre { get; set; }
    public int? Year { get; set; }
    public decimal? MinRating { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}