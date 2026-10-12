using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserManagementApi.Data;
using UserManagementApi.Models;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services.Interfaces;
using UserManagementApi.Services.Storage;

namespace UserManagementApi.Services;

public class MovieService : IMovieService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MovieService> _logger;
    private readonly IImageStorage _imageStorage;
    private readonly long _maxPosterBytes;

    public MovieService(
        AppDbContext context,
        IMapper mapper,
        ILogger<MovieService> logger,
        IImageStorage imageStorage,
        IOptions<ImageStorageOptions> options)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _imageStorage = imageStorage;
        _maxPosterBytes = options.Value.MaxPosterBytes;
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
            // Normalise and validate PosterUrl (AC 6)
            dto.PosterUrl = string.IsNullOrWhiteSpace(dto.PosterUrl) ? null : dto.PosterUrl;
            if (!IsAcceptablePosterUrl(dto.PosterUrl, null))
                return new MovieResponseDto { Success = false, Message = "PosterUrl must be an absolute http(s) URL; use PUT /api/movies/{id}/poster to upload an image" };

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

            // Normalise and validate PosterUrl (AC 6)
            dto.PosterUrl = string.IsNullOrWhiteSpace(dto.PosterUrl) ? null : dto.PosterUrl;
            if (!IsAcceptablePosterUrl(dto.PosterUrl, movie.PosterUrl))
                return new MovieResponseDto { Success = false, Message = "PosterUrl must be an absolute http(s) URL; use PUT /api/movies/{id}/poster to upload an image" };

            var oldUrl = movie.PosterUrl;
            _mapper.Map(dto, movie);
            movie.UpdatedAt = DateTime.UtcNow;

            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();

            // Clean up old managed poster if URL changed
            if (oldUrl != movie.PosterUrl)
                await _imageStorage.DeletePosterAsync(oldUrl);

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

            var posterUrl = movie.PosterUrl;
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            await _imageStorage.DeletePosterAsync(posterUrl);

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

    public async Task<MovieResponseDto> SetPosterAsync(int id, IFormFile? file, CancellationToken ct = default)
    {
        try
        {
            // AC 3: validate everything before touching the DB or storage
            if (file is null || file.Length == 0)
                return new MovieResponseDto { Success = false, Message = "No file was uploaded" };

            if (file.Length > _maxPosterBytes)
                return new MovieResponseDto { Success = false, Message = "File is too large. Maximum size is 5 MB" };

            var movie = await _context.Movies.FindAsync([id], ct);
            if (movie is null)
                return new MovieResponseDto { Success = false, Message = "Movie not found" };

            await using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer, ct);
            buffer.Position = 0;

            var kind = ImageFormat.Detect(buffer);
            if (kind is null)
                return new MovieResponseDto { Success = false, Message = "Unsupported image format. Allowed: JPEG, PNG, WebP" };

            buffer.Position = 0;
            var oldUrl = movie.PosterUrl;
            var newUrl = await _imageStorage.SavePosterAsync(buffer, kind.Value, ct);

            movie.PosterUrl = newUrl;
            movie.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch
            {
                await _imageStorage.DeletePosterAsync(newUrl, ct);
                throw;
            }

            // Best-effort cleanup of the old managed poster (AC 5: no-op for external URLs)
            if (oldUrl != newUrl)
                await _imageStorage.DeletePosterAsync(oldUrl, ct);

            _logger.LogInformation("Poster set for movie {MovieId}", id);

            return new MovieResponseDto
            {
                Success = true,
                Message = "Poster uploaded successfully",
                Data = _mapper.Map<MovieDto>(movie)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading poster for movie {MovieId}", id);
            return new MovieResponseDto { Success = false, Message = "An error occurred while uploading poster" };
        }
    }

    public async Task<MovieResponseDto> RemovePosterAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var movie = await _context.Movies.FindAsync([id], ct);
            if (movie is null)
                return new MovieResponseDto { Success = false, Message = "Movie not found" };

            var oldUrl = movie.PosterUrl;
            if (string.IsNullOrWhiteSpace(oldUrl))
                return new MovieResponseDto { Success = true, Message = "Poster removed successfully", Data = _mapper.Map<MovieDto>(movie) };

            movie.PosterUrl = null;
            movie.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            await _imageStorage.DeletePosterAsync(oldUrl, ct);

            return new MovieResponseDto
            {
                Success = true,
                Message = "Poster removed successfully",
                Data = _mapper.Map<MovieDto>(movie)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing poster for movie {MovieId}", id);
            return new MovieResponseDto { Success = false, Message = "An error occurred while removing poster" };
        }
    }

    private bool IsAcceptablePosterUrl(string? url, string? currentUrl)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        if (url == currentUrl) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == "http" || u.Scheme == "https");
    }
}