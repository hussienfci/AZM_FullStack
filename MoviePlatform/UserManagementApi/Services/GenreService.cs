using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Models;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Services;

public class GenreService : IGenreService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GenreService> _logger;

    public GenreService(AppDbContext context, IMapper mapper, ILogger<GenreService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GenresListResponseDto> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Genres
                .AsNoTracking()
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name);

            var totalCount = await query.CountAsync();
            var genres = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new GenresListResponseDto
            {
                Success = true,
                Message = "Genres retrieved successfully",
                Data = _mapper.Map<List<GenreDto>>(genres),
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving genres");
            return new GenresListResponseDto { Success = false, Message = "An error occurred while retrieving genres" };
        }
    }

    public async Task<GenreResponseDto> GetByIdAsync(int id)
    {
        try
        {
            var genre = await _context.Genres.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (genre == null)
                return new GenreResponseDto { Success = false, Message = "Genre not found" };

            return new GenreResponseDto
            {
                Success = true,
                Message = "Genre retrieved successfully",
                Data = _mapper.Map<GenreDto>(genre)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving genre with ID {GenreId}", id);
            return new GenreResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<GenreResponseDto> GetByNameAsync(string name)
    {
        try
        {
            var genre = await _context.Genres
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Name.ToLower() == name.ToLower());

            if (genre == null)
                return new GenreResponseDto { Success = false, Message = "Genre not found" };

            return new GenreResponseDto
            {
                Success = true,
                Message = "Genre retrieved successfully",
                Data = _mapper.Map<GenreDto>(genre)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving genre with name {Name}", name);
            return new GenreResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<GenreResponseDto> CreateAsync(CreateGenreDto dto)
    {
        try
        {
            if (await NameExistsAsync(dto.Name))
                return new GenreResponseDto { Success = false, Message = "Genre name already exists" };

            var genre = _mapper.Map<Genre>(dto);
            genre.CreatedAt = DateTime.UtcNow;

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Genre created with ID {GenreId}", genre.Id);

            return new GenreResponseDto
            {
                Success = true,
                Message = "Genre created successfully",
                Data = _mapper.Map<GenreDto>(genre)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating genre");
            return new GenreResponseDto { Success = false, Message = "An error occurred while creating genre" };
        }
    }

    public async Task<GenreResponseDto> UpdateAsync(int id, UpdateGenreDto dto)
    {
        try
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return new GenreResponseDto { Success = false, Message = "Genre not found" };

            _mapper.Map(dto, genre);
            genre.UpdatedAt = DateTime.UtcNow;

            _context.Genres.Update(genre);
            await _context.SaveChangesAsync();

            return new GenreResponseDto
            {
                Success = true,
                Message = "Genre updated successfully",
                Data = _mapper.Map<GenreDto>(genre)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating genre with ID {GenreId}", id);
            return new GenreResponseDto { Success = false, Message = "An error occurred while updating genre" };
        }
    }

    public async Task<GenreResponseDto> DeleteAsync(int id)
    {
        try
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return new GenreResponseDto { Success = false, Message = "Genre not found" };

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Genre with ID {GenreId} deleted", id);

            return new GenreResponseDto { Success = true, Message = "Genre deleted successfully" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting genre with ID {GenreId}", id);
            return new GenreResponseDto { Success = false, Message = "An error occurred while deleting genre" };
        }
    }

    public async Task<GenreResponseDto> ToggleActiveStatusAsync(int id)
    {
        try
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return new GenreResponseDto { Success = false, Message = "Genre not found" };

            genre.IsActive = !genre.IsActive;
            genre.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new GenreResponseDto
            {
                Success = true,
                Message = $"Genre {(genre.IsActive ? "activated" : "deactivated")} successfully",
                Data = _mapper.Map<GenreDto>(genre)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for genre {GenreId}", id);
            return new GenreResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _context.Genres.AnyAsync(g => g.Name.ToLower() == name.ToLower());
    }
}
