namespace UserManagementApi.Models.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
}

public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public UserRole Role { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class LoginResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserDto? User { get; set; }
}
public class UserResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserDto? Data { get; set; }
}

public class UsersListResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<UserDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}