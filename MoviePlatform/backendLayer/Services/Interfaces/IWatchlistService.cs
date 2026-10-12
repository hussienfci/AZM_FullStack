using UserManagementApi.Models.DTOs;

namespace UserManagementApi.Services.Interfaces;

public interface IWatchlistService
{
    Task<WatchlistListResponseDto> GetAllAsync(int page = 1, int pageSize = 10);
    Task<WatchlistResponseDto> GetByIdAsync(int id);
    Task<UserWatchlistResponseDto> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10);
    Task<WatchlistResponseDto> CreateAsync(CreateWatchlistDto dto);
    Task<WatchlistResponseDto> UpdateAsync(int id, UpdateWatchlistDto dto);
    Task<WatchlistResponseDto> MarkAsWatchedAsync(int id);
    Task<WatchlistResponseDto> DeleteAsync(int id);
    Task<WatchlistResponseDto> ToggleActiveStatusAsync(int id);
    Task<bool> ExistsInWatchlistAsync(int userId, int movieId);
}
