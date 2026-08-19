using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, IAuthService authService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }

    // ── AUTH ENDPOINTS ──

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new LoginResponseDto { Success = false, Message = "Invalid request" });

        var response = await _authService.LoginAsync(dto);

        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new LoginResponseDto { Success = false, Message = "Invalid request" });

        var response = await _authService.RegisterAsync(dto);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst("userId")?.Value;
        var email = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
        var firstName = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.GivenName)?.Value;
        var lastName = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.FamilyName)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new
        {
            UserId = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role
        });
    }

    // ── CRUD ENDPOINTS (now protected) ──

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UsersListResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsersListResponseDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _userService.GetAllAsync(page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetById(int id)
    {
        var response = await _userService.GetByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpGet("email/{email}")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetByEmail(string email)
    {
        var response = await _userService.GetByEmailAsync(email);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponseDto>> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new UserResponseDto { Success = false, Message = "Invalid model state" });

        var response = await _userService.CreateAsync(dto);

        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponseDto>> Update(int id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new UserResponseDto { Success = false, Message = "Invalid model state" });

        var response = await _userService.UpdateAsync(id, dto);

        if (!response.Success)
            return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> Delete(int id)
    {
        var response = await _userService.DeleteAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPatch("{id:int}/toggle-status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> ToggleStatus(int id)
    {
        var response = await _userService.ToggleActiveStatusAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
