using UserManagementApi.Models;
using UserManagementApi.Models.DTOs;

namespace UserManagementApi.Services.Interfaces;

public interface IUserService
{
    Task<UsersListResponseDto> GetAllAsync(int page = 1, int pageSize = 10);
    Task<UserResponseDto> GetByIdAsync(int id);
    Task<UserResponseDto> GetByEmailAsync(string email);
    Task<UserResponseDto> CreateAsync(CreateUserDto dto);
    Task<UserResponseDto> UpdateAsync(int id, UpdateUserDto dto);
    Task<UserResponseDto> DeleteAsync(int id);
    Task<UserResponseDto> ToggleActiveStatusAsync(int id);
    Task<bool> EmailExistsAsync(string email);
}