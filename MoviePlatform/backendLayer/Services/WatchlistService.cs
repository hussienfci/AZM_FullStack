using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Models;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Services;

public class WatchlistService : IWatchlistService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<WatchlistService> _logger;

    public WatchlistService(AppDbContext context, IMapper mapper, ILogger<WatchlistService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WatchlistListResponseDto> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Watchlists
                .AsNoTracking()
                .Include(w => w.User)
                .Include(w => w.Movie)
                .Where(w => w.IsActive)
                .OrderByDescending(w => w.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new WatchlistListResponseDto
            {
                Success = true,
                Message = "Watchlist items retrieved successfully",
                Data = _mapper.Map<List<WatchlistDto>>(items),
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving watchlist items");
            return new WatchlistListResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<WatchlistResponseDto> GetByIdAsync(int id)
    {
        try
        {
            var item = await _context.Watchlists
                .AsNoTracking()
                .Include(w => w.User)
                .Include(w => w.Movie)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (item == null)
                return new WatchlistResponseDto { Success = false, Message = "Watchlist item not found" };

            return new WatchlistResponseDto
            {
                Success = true,
                Message = "Watchlist item retrieved successfully",
                Data = _mapper.Map<WatchlistDto>(item)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving watchlist item {WatchlistId}", id);
            return new WatchlistResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<UserWatchlistResponseDto> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Watchlists
                .AsNoTracking()
                .Include(w => w.Movie)
                .Where(w => w.UserId == userId && w.IsActive)
                .OrderByDescending(w => w.CreatedAt);

            var totalMovies = await query.CountAsync();
            var watchedCount = await query.CountAsync(w => w.IsWatched);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new UserWatchlistResponseDto
            {
                Success = true,
                Message = "User watchlist retrieved successfully",
                Data = _mapper.Map<List<WatchlistDto>>(items),
                TotalMovies = totalMovies,
                WatchedCount = watchedCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving watchlist for user {UserId}", userId);
            return new UserWatchlistResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<WatchlistResponseDto> CreateAsync(CreateWatchlistDto dto)
    {
        try
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId && u.IsActive);
            if (!userExists)
                return new WatchlistResponseDto { Success = false, Message = "User not found" };

            var movieExists = await _context.Movies.AnyAsync(m => m.Id == dto.MovieId && m.IsActive);
            if (!movieExists)
                return new WatchlistResponseDto { Success = false, Message = "Movie not found" };

            if (await ExistsInWatchlistAsync(dto.UserId, dto.MovieId))
                return new WatchlistResponseDto { Success = false, Message = "Movie already in watchlist" };

            var watchlist = _mapper.Map<Watchlist>(dto);
            watchlist.CreatedAt = DateTime.UtcNow;

            _context.Watchlists.Add(watchlist);
            await _context.SaveChangesAsync();

            var created = await _context.Watchlists
                .Include(w => w.User)
                .Include(w => w.Movie)
                .FirstAsync(w => w.Id == watchlist.Id);

            _logger.LogInformation("Watchlist item created with ID {WatchlistId}", watchlist.Id);

            return new WatchlistResponseDto
            {
                Success = true,
                Message = "Added to watchlist successfully",
                Data = _mapper.Map<WatchlistDto>(created)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating watchlist item");
            return new WatchlistResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<WatchlistResponseDto> UpdateAsync(int id, UpdateWatchlistDto dto)
    {
        try
        {
            var item = await _context.Watchlists.FindAsync(id);
            if (item == null)
                return new WatchlistResponseDto { Success = false, Message = "Watchlist item not found" };

            _mapper.Map(dto, item);
            item.UpdatedAt = DateTime.UtcNow;

            _context.Watchlists.Update(item);
            await _context.SaveChangesAsync();

            var updated = await _context.Watchlists
                .Include(w => w.User)
                .Include(w => w.Movie)
                .FirstAsync(w => w.Id == id);

            return new WatchlistResponseDto
            {
                Success = true,
                Message = "Watchlist item updated successfully",
                Data = _mapper.Map<WatchlistDto>(updated)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating watchlist item {WatchlistId}", id);
            return new WatchlistResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<WatchlistResponseDto> MarkAsWatchedAsync(int id)
    {
        try
        {
            var item = await _context.Watchlists.FindAsync(id);
            if (item == null)
                return new WatchlistResponseDto { Success = false, Message = "Watchlist item not found" };

            item.IsWatched = true;
            item.WatchedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updated = await _context.Watchlists
                .Include(w => w.User)
                .Include(w => w.Movie)
                .FirstAsync(w => w.Id == id);

            return new WatchlistResponseDto
            {
                Success = true,
                Message = "Marked as watched successfully",
                Data = _mapper.Map<WatchlistDto>(updated)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking watchlist item {WatchlistId} as watched", id);
            return new WatchlistResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<WatchlistResponseDto> DeleteAsync(int id)
    {
        try
        {
            var item = await _context.Watchlists.FindAsync(id);
            if (item == null)
                return new WatchlistResponseDto { Success = false, Message = "Watchlist item not found" };

            _context.Watchlists.Remove(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Watchlist item {WatchlistId} deleted", id);

            return new WatchlistResponseDto { Success = true, Message = "Removed from watchlist successfully" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting watchlist item {WatchlistId}", id);
            return new WatchlistResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<WatchlistResponseDto> ToggleActiveStatusAsync(int id)
    {
        try
        {
            var item = await _context.Watchlists.FindAsync(id);
            if (item == null)
                return new WatchlistResponseDto { Success = false, Message = "Watchlist item not found" };

            item.IsActive = !item.IsActive;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new WatchlistResponseDto
            {
                Success = true,
                Message = $"Watchlist item {(item.IsActive ? "activated" : "deactivated")} successfully",
                Data = _mapper.Map<WatchlistDto>(item)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for watchlist item {WatchlistId}", id);
            return new WatchlistResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<bool> ExistsInWatchlistAsync(int userId, int movieId)
    {
        return await _context.Watchlists.AnyAsync(w => w.UserId == userId && w.MovieId == movieId);
    }
}
