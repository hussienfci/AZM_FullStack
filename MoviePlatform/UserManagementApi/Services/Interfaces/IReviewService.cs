using UserManagementApi.Models.DTOs;

namespace UserManagementApi.Services.Interfaces;

public interface IReviewService
{
    Task<ReviewsListResponseDto> GetAllAsync(int page = 1, int pageSize = 10);
    Task<ReviewResponseDto> GetByIdAsync(int id);
    Task<MovieReviewsResponseDto> GetByMovieIdAsync(int movieId, int page = 1, int pageSize = 10);
    Task<ReviewsListResponseDto> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10);
    Task<ReviewResponseDto> CreateAsync(CreateReviewDto dto);
    Task<ReviewResponseDto> UpdateAsync(int id, UpdateReviewDto dto);
    Task<ReviewResponseDto> DeleteAsync(int id);
    Task<ReviewResponseDto> ToggleActiveStatusAsync(int id);
    Task<bool> UserHasReviewedMovieAsync(int userId, int movieId);
}
