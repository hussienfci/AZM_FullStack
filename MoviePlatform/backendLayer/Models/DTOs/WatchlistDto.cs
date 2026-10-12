namespace UserManagementApi.Models.DTOs;

public class WatchlistDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string MoviePosterUrl { get; set; } = string.Empty;
    public bool IsWatched { get; set; }
    public DateTime? WatchedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateWatchlistDto
{
    public int UserId { get; set; }
    public int MovieId { get; set; }
}

public class UpdateWatchlistDto
{
    public bool IsWatched { get; set; }
    public DateTime? WatchedAt { get; set; }
}

public class WatchlistResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public WatchlistDto? Data { get; set; }
}

public class WatchlistListResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<WatchlistDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}

public class UserWatchlistResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<WatchlistDto> Data { get; set; } = new();
    public int TotalMovies { get; set; }
    public int WatchedCount { get; set; }
}
