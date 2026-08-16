using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Models;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(AppDbContext context, IMapper mapper, ILogger<ReviewService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReviewsListResponseDto> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Movie)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new ReviewsListResponseDto
            {
                Success = true,
                Message = "Reviews retrieved successfully",
                Data = _mapper.Map<List<ReviewDto>>(reviews),
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reviews");
            return new ReviewsListResponseDto { Success = false, Message = "An error occurred while retrieving reviews" };
        }
    }

    public async Task<ReviewResponseDto> GetByIdAsync(int id)
    {
        try
        {
            var review = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return new ReviewResponseDto { Success = false, Message = "Review not found" };

            return new ReviewResponseDto
            {
                Success = true,
                Message = "Review retrieved successfully",
                Data = _mapper.Map<ReviewDto>(review)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving review with ID {ReviewId}", id);
            return new ReviewResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<MovieReviewsResponseDto> GetByMovieIdAsync(int movieId, int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.MovieId == movieId && r.IsActive)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var averageRating = await _context.Reviews
                .Where(r => r.MovieId == movieId && r.IsActive)
                .AverageAsync(r => (decimal?)r.Rating) ?? 0;

            return new MovieReviewsResponseDto
            {
                Success = true,
                Message = "Movie reviews retrieved successfully",
                Data = _mapper.Map<List<ReviewDto>>(reviews),
                AverageRating = Math.Round(averageRating, 1),
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reviews for movie {MovieId}", movieId);
            return new MovieReviewsResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<ReviewsListResponseDto> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Reviews
                .AsNoTracking()
                .Include(r => r.Movie)
                .Where(r => r.UserId == userId && r.IsActive)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new ReviewsListResponseDto
            {
                Success = true,
                Message = "User reviews retrieved successfully",
                Data = _mapper.Map<List<ReviewDto>>(reviews),
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reviews for user {UserId}", userId);
            return new ReviewsListResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<ReviewResponseDto> CreateAsync(CreateReviewDto dto)
    {
        try
        {
            // Check if user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId && u.IsActive);
            if (!userExists)
                return new ReviewResponseDto { Success = false, Message = "User not found" };

            // Check if movie exists
            var movieExists = await _context.Movies.AnyAsync(m => m.Id == dto.MovieId && m.IsActive);
            if (!movieExists)
                return new ReviewResponseDto { Success = false, Message = "Movie not found" };

            // Check if user already reviewed this movie
            if (await UserHasReviewedMovieAsync(dto.UserId, dto.MovieId))
                return new ReviewResponseDto { Success = false, Message = "User has already reviewed this movie" };

            // Validate rating
            if (dto.Rating < 1 || dto.Rating > 10)
                return new ReviewResponseDto { Success = false, Message = "Rating must be between 1 and 10" };

            var review = _mapper.Map<Review>(dto);
            review.CreatedAt = DateTime.UtcNow;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Load related data for response
            var createdReview = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Movie)
                .FirstAsync(r => r.Id == review.Id);

            _logger.LogInformation("Review created with ID {ReviewId}", review.Id);

            return new ReviewResponseDto
            {
                Success = true,
                Message = "Review created successfully",
                Data = _mapper.Map<ReviewDto>(createdReview)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return new ReviewResponseDto { Success = false, Message = "An error occurred while creating review" };
        }
    }

    public async Task<ReviewResponseDto> UpdateAsync(int id, UpdateReviewDto dto)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
                return new ReviewResponseDto { Success = false, Message = "Review not found" };

            // Validate rating
            if (dto.Rating < 1 || dto.Rating > 10)
                return new ReviewResponseDto { Success = false, Message = "Rating must be between 1 and 10" };

            _mapper.Map(dto, review);
            review.UpdatedAt = DateTime.UtcNow;

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            // Load related data for response
            var updatedReview = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Movie)
                .FirstAsync(r => r.Id == id);

            return new ReviewResponseDto
            {
                Success = true,
                Message = "Review updated successfully",
                Data = _mapper.Map<ReviewDto>(updatedReview)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review with ID {ReviewId}", id);
            return new ReviewResponseDto { Success = false, Message = "An error occurred while updating review" };
        }
    }

    public async Task<ReviewResponseDto> DeleteAsync(int id)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
                return new ReviewResponseDto { Success = false, Message = "Review not found" };

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Review with ID {ReviewId} deleted", id);

            return new ReviewResponseDto { Success = true, Message = "Review deleted successfully" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review with ID {ReviewId}", id);
            return new ReviewResponseDto { Success = false, Message = "An error occurred while deleting review" };
        }
    }

    public async Task<ReviewResponseDto> ToggleActiveStatusAsync(int id)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
                return new ReviewResponseDto { Success = false, Message = "Review not found" };

            review.IsActive = !review.IsActive;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ReviewResponseDto
            {
                Success = true,
                Message = $"Review {(review.IsActive ? "activated" : "deactivated")} successfully",
                Data = _mapper.Map<ReviewDto>(review)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for review {ReviewId}", id);
            return new ReviewResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<bool> UserHasReviewedMovieAsync(int userId, int movieId)
    {
        return await _context.Reviews.AnyAsync(r => r.UserId == userId && r.MovieId == movieId);
    }
}  