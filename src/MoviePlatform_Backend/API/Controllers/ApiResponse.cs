using System.Collections.Generic;

namespace MoviePlatform.API.Controllers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public PaginationMeta? Pagination { get; set; }
}

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public object? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Success<T>(T data)
    {
        return new ApiResponse<T> { Success = true, Data = data };
    }

    public static ApiResponse Success()
    {
        return new ApiResponse { IsSuccess = true };
    }

    public static ApiResponse Failure(List<string> errors)
    {
        return new ApiResponse { IsSuccess = false, Errors = errors };
    }

    public static ApiResponse Failure(string error)
    {
        return new ApiResponse { IsSuccess = false, Errors = new List<string> { error } };
    }
}

public class PaginationMeta
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}
