using System.Security.Claims;
using AutoMapper;
using EkubCircle.Application.Commands.Circles;
using EkubCircle.Application.Queries.Circles;
using EkubCircle.Application.DTOs.Circles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkubCircle.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CirclesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public CirclesController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
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
            var command = new CreateCircleCommand(userId, request.Name, request.ContributionAmount, request.MeetingLabel);
            var circle = await _sender.Send(command);
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
        var query = new GetUserCirclesQuery(userId, status);
        var circles = await _sender.Send(query);
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
            var query = new GetCircleByIdQuery(id, userId);
            var circle = await _sender.Send(query);
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
    /// Get all members of a circle
    /// </summary>
    [HttpGet("{id:int}/members")]
    [ProducesResponseType(typeof(List<CircleMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCircleMembers(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetCircleByIdQuery(id, userId);
            var circle = await _sender.Send(query);
            return Ok(circle.Members);
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
            var command = new AddMemberCommand(id, userId, request.Email);
            var member = await _sender.Send(command);
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
            var command = new RemoveMemberCommand(id, userId, memberId);
            await _sender.Send(command);
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
            var command = new StartCircleCommand(id, userId);
            var circle = await _sender.Send(command);
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

    /// <summary>
    /// Extra Credit: Completed-circle audit summary and metrics report.
    /// </summary>
    [HttpGet("{id:int}/summary")]
    [ProducesResponseType(typeof(CircleSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCircleSummary(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetCircleSummaryQuery(id, userId);
            var summary = await _sender.Send(query);
            return Ok(summary);
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
}
