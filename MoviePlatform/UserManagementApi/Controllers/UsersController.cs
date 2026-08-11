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
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(UsersListResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsersListResponseDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _userService.GetAllAsync(page, pageSize);
        return Ok(response);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetById(int id)
    {
        var response = await _userService.GetByIdAsync(id);
        
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetByEmail(string email)
    {
        var response = await _userService.GetByEmailAsync(email);
        
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
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

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id:int}")]
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

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> Delete(int id)
    {
        var response = await _userService.DeleteAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// Toggle user active status
    /// </summary>
    [HttpPatch("{id:int}/toggle-status")]
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