using System.Security.Claims;
using AutoMapper;
using EkubCircle.API.Application.Commands.Auth;
using EkubCircle.API.Application.Queries.Auth;
using EkubCircle.API.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkubCircle.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public AuthController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and Password are required." });
        }

        try
        {
            var command = _mapper.Map<RegisterUserCommand>(request);
            var result = await _sender.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Authenticate and receive a JWT Bearer token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and Password are required." });
        }

        try
        {
            var command = _mapper.Map<LoginUserCommand>(request);
            var result = await _sender.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieve current authenticated user profile
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int userId))
        {
            return Unauthorized(new { message = "Invalid user token claims." });
        }

        try
        {
            var query = new GetCurrentUserQuery(userId);
            var user = await _sender.Send(query);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
