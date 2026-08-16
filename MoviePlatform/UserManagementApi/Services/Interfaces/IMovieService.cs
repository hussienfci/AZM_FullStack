using UserManagementApi.Models.DTOs;

namespace UserManagementApi.Services.Interfaces;

public interface IMovieService
{
    Task<MoviesListResponseDto> GetAllAsync(int page = 1, int pageSize = 10);
    Task<MovieResponseDto> GetByIdAsync(int id);
    Task<MoviesListResponseDto> SearchAsync(MovieSearchDto searchDto);
    Task<MovieResponseDto> CreateAsync(CreateMovieDto dto);
    Task<MovieResponseDto> UpdateAsync(int id, UpdateMovieDto dto);
    Task<MovieResponseDto> DeleteAsync(int id);
    Task<MovieResponseDto> ToggleActiveStatusAsync(int id);
    Task<MoviesListResponseDto> GetByGenreAsync(string genre, int page = 1, int pageSize = 10);
    Task<MoviesListResponseDto> GetTopRatedAsync(int count = 10);
}