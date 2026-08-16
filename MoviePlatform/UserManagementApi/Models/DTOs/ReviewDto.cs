namespace UserManagementApi.Models.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewDto
{
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class UpdateReviewDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class ReviewResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ReviewDto? Data { get; set; }
}

public class ReviewsListResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ReviewDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}

public class MovieReviewsResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ReviewDto> Data { get; set; } = new();
    public decimal AverageRating { get; set; }
    public int TotalCount { get; set; }
}