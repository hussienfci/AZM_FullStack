using UserManagementApi.Models.DTOs;

namespace UserManagementApi.Services.Interfaces;

public interface IGenreService
{
    Task<GenresListResponseDto> GetAllAsync(int page = 1, int pageSize = 10);
    Task<GenreResponseDto> GetByIdAsync(int id);
    Task<GenreResponseDto> GetByNameAsync(string name);
    Task<GenreResponseDto> CreateAsync(CreateGenreDto dto);
    Task<GenreResponseDto> UpdateAsync(int id, UpdateGenreDto dto);
    Task<GenreResponseDto> DeleteAsync(int id);
    Task<GenreResponseDto> ToggleActiveStatusAsync(int id);
    Task<bool> NameExistsAsync(string name);
}
