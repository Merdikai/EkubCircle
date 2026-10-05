using System.Security.Claims;
using EkubCircle.API.DTOs.Circles;
using EkubCircle.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkubCircle.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CirclesController : ControllerBase
{
    private readonly ICircleService _circleService;

    public CirclesController(ICircleService circleService)
    {
        _circleService = circleService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(claim!);
    }

    /// <summary>
    /// Create a new Ekub circle
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CircleDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCircle([FromBody] CreateCircleRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Circle name is required." });
        }

        try
        {
            var userId = GetCurrentUserId();
            var circle = await _circleService.CreateCircleAsync(userId, request);
            return CreatedAtAction(nameof(GetCircleById), new { id = circle.Id }, circle);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all circles the current user belongs to
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CircleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCircles([FromQuery] string? status)
    {
        var userId = GetCurrentUserId();
        var circles = await _circleService.GetUserCirclesAsync(userId, status);
        return Ok(circles);
    }

    /// <summary>
    /// Get details of a specific circle including members and current round
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CircleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCircleById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var circle = await _circleService.GetCircleDetailsAsync(id, userId);
            return Ok(circle);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Add a member to a forming circle by registered email (Organizer only)
    /// </summary>
    [HttpPost("{id:int}/members")]
    [ProducesResponseType(typeof(CircleMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Member email is required." });
        }

        try
        {
            var userId = GetCurrentUserId();
            var member = await _circleService.AddMemberAsync(id, userId, request);
            return StatusCode(StatusCodes.Status201Created, member);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a member from a forming circle (Organizer only)
    /// </summary>
    [HttpDelete("{id:int}/members/{memberId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(int id, int memberId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _circleService.RemoveMemberAsync(id, userId, memberId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Start the circle: locks member list, sets deterministic payout order, and generates rounds
    /// </summary>
    [HttpPost("{id:int}/start")]
    [ProducesResponseType(typeof(CircleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartCircle(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var circle = await _circleService.StartCircleAsync(id, userId);
            return Ok(circle);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
