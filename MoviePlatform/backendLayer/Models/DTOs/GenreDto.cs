namespace UserManagementApi.Models.DTOs;

public class GenreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateGenreDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateGenreDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class GenreResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public GenreDto? Data { get; set; }
}

public class GenresListResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<GenreDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}
