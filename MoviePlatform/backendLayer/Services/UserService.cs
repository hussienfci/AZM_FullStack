using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Models;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, IMapper mapper, ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UsersListResponseDto> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt);
            var totalCount = await query.CountAsync();
            
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new UsersListResponseDto
            {
                Success = true,
                Message = "Users retrieved successfully",
                Data = _mapper.Map<List<UserDto>>(users),
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return new UsersListResponseDto
            {
                Success = false,
                Message = "An error occurred while retrieving users"
            };
        }
    }

    public async Task<UserResponseDto> GetByIdAsync(int id)
    {
        try
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            
            if (user == null)
                return new UserResponseDto { Success = false, Message = "User not found" };

            return new UserResponseDto
            {
                Success = true,
                Message = "User retrieved successfully",
                Data = _mapper.Map<UserDto>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            return new UserResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<UserResponseDto> GetByEmailAsync(string email)
    {
        try
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
                return new UserResponseDto { Success = false, Message = "User not found" };

            return new UserResponseDto
            {
                Success = true,
                Message = "User retrieved successfully",
                Data = _mapper.Map<UserDto>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with email {Email}", email);
            return new UserResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
    {
        try
        {
            if (await EmailExistsAsync(dto.Email))
                return new UserResponseDto { Success = false, Message = "Email already exists" };

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created with ID {UserId}", user.Id);

            return new UserResponseDto
            {
                Success = true,
                Message = "User created successfully",
                Data = _mapper.Map<UserDto>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return new UserResponseDto { Success = false, Message = "An error occurred while creating user" };
        }
    }

    public async Task<UserResponseDto> UpdateAsync(int id, UpdateUserDto dto)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                return new UserResponseDto { Success = false, Message = "User not found" };

            _mapper.Map(dto, user);
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Success = true,
                Message = "User updated successfully",
                Data = _mapper.Map<UserDto>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID {UserId}", id);
            return new UserResponseDto { Success = false, Message = "An error occurred while updating user" };
        }
    }

    public async Task<UserResponseDto> DeleteAsync(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                return new UserResponseDto { Success = false, Message = "User not found" };

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User with ID {UserId} deleted", id);

            return new UserResponseDto
            {
                Success = true,
                Message = "User deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            return new UserResponseDto { Success = false, Message = "An error occurred while deleting user" };
        }
    }

    public async Task<UserResponseDto> ToggleActiveStatusAsync(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
                return new UserResponseDto { Success = false, Message = "User not found" };

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Success = true,
                Message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully",
                Data = _mapper.Map<UserDto>(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for user {UserId}", id);
            return new UserResponseDto { Success = false, Message = "An error occurred" };
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
}