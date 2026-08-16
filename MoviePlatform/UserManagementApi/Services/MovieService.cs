using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Models;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Services;

public class MovieService : IMovieService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MovieService> _logger;

    public MovieService(AppDbContext context, IMapper mapper, ILogger<MovieService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<MoviesListResponseDto> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Movies
                .AsNoTracking()
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.CreatedAt);

            var totalCount = await query.CountAsync();
            var movies = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new MoviesListResponseDto
            {
                Success = true,
                Message = "Movies retrieved successfully",
                Data = _mapper.Map<List<MovieDto>>(movies),
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movies");
            return new MoviesListResponseDto
            {
                Success = false,
                Message = "An error occurred while retrieving movies"
            };
        }
    }

    public async Task<MovieResponseDto> GetByIdAsync(int id)
    {
        try
        {
            var movie = await _context.Movies
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return new MovieResponseDto { Success = false, Message = "Movie not found" };

            return new MovieResponseDto
            {
                Success = true,
                Message = "Movie retrieved successfully",
                Data = _mapper.Map<MovieDto>(movie)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movie with ID {MovieId}", id);
            return new MovieResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<MoviesListResponseDto> SearchAsync(MovieSearchDto searchDto)
    {
        try
        {
            var query = _context.Movies.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
            {
                var term = searchDto.SearchTerm.ToLower();
                query = query.Where(m =>
                    m.Title.ToLower().Contains(term) ||
                    (m.Description != null && m.Description.ToLower().Contains(term)) ||
                    (m.Director != null && m.Director.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Genre))
                query = query.Where(m => m.Genre.ToLower() == searchDto.Genre.ToLower());

            if (searchDto.Year.HasValue)
                query = query.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == searchDto.Year.Value);

            if (searchDto.MinRating.HasValue)
                query = query.Where(m => m.Rating.HasValue && m.Rating >= searchDto.MinRating.Value);

            query = query.Where(m => m.IsActive).OrderByDescending(m => m.Rating);

            var totalCount = await query.CountAsync();
            var movies = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return new MoviesListResponseDto
            {
                Success = true,
                Message = "Search completed successfully",
                Data = _mapper.Map<List<MovieDto>>(movies),
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching movies");
            return new MoviesListResponseDto
            {
                Success = false,
                Message = "An error occurred while searching movies"
            };
        }
    }

    public async Task<MovieResponseDto> CreateAsync(CreateMovieDto dto)
    {
        try
        {
            var movie = _mapper.Map<Movie>(dto);
            movie.CreatedAt = DateTime.UtcNow;

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Movie created with ID {MovieId}", movie.Id);

            return new MovieResponseDto
            {
                Success = true,
                Message = "Movie created successfully",
                Data = _mapper.Map<MovieDto>(movie)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating movie");
            return new MovieResponseDto { Success = false, Message = "An error occurred while creating movie" };
        }
    }

    public async Task<MovieResponseDto> UpdateAsync(int id, UpdateMovieDto dto)
    {
        try
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return new MovieResponseDto { Success = false, Message = "Movie not found" };

            _mapper.Map(dto, movie);
            movie.UpdatedAt = DateTime.UtcNow;

            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();

            return new MovieResponseDto
            {
                Success = true,
                Message = "Movie updated successfully",
                Data = _mapper.Map<MovieDto>(movie)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating movie with ID {MovieId}", id);
            return new MovieResponseDto { Success = false, Message = "An error occurred while updating movie" };
        }
    }

    public async Task<MovieResponseDto> DeleteAsync(int id)
    {
        try
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return new MovieResponseDto { Success = false, Message = "Movie not found" };

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Movie with ID {MovieId} deleted", id);

            return new MovieResponseDto
            {
                Success = true,
                Message = "Movie deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting movie with ID {MovieId}", id);
            return new MovieResponseDto { Success = false, Message = "An error occurred while deleting movie" };
        }
    }

    public async Task<MovieResponseDto> ToggleActiveStatusAsync(int id)
    {
        try
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return new MovieResponseDto { Success = false, Message = "Movie not found" };

            movie.IsActive = !movie.IsActive;
            movie.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new MovieResponseDto
            {
                Success = true,
                Message = $"Movie {(movie.IsActive ? "activated" : "deactivated")} successfully",
                Data = _mapper.Map<MovieDto>(movie)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for movie {MovieId}", id);
            return new MovieResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<MoviesListResponseDto> GetByGenreAsync(string genre, int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Movies
                .AsNoTracking()
                .Where(m => m.Genre.ToLower() == genre.ToLower() && m.IsActive)
                .OrderByDescending(m => m.Rating);

            var totalCount = await query.CountAsync();
            var movies = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new MoviesListResponseDto
            {
                Success = true,
                Message = "Movies retrieved successfully",
                Data = _mapper.Map<List<MovieDto>>(movies),
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movies by genre {Genre}", genre);
            return new MoviesListResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<MoviesListResponseDto> GetTopRatedAsync(int count = 10)
    {
        try
        {
            var movies = await _context.Movies
                .AsNoTracking()
                .Where(m => m.IsActive && m.Rating.HasValue)
                .OrderByDescending(m => m.Rating)
                .Take(count)
                .ToListAsync();

            return new MoviesListResponseDto
            {
                Success = true,
                Message = "Top rated movies retrieved successfully",
                Data = _mapper.Map<List<MovieDto>>(movies),
                TotalCount = movies.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top rated movies");
            return new MoviesListResponseDto { Success = false, Message = "An error occurred" };
        }
    }
}