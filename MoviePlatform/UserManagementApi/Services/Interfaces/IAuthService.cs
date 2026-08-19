using UserManagementApi.Models.DTOs;

namespace UserManagementApi.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<LoginResponseDto> RegisterAsync(CreateUserDto dto);
    string GenerateJwtToken(UserManagementApi.Models.User user);
}
