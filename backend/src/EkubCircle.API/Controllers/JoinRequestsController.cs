using System.Security.Claims;
using AutoMapper;
using EkubCircle.Application.Commands.JoinRequests;
using EkubCircle.Application.DTOs.JoinRequests;
using EkubCircle.Application.Queries.JoinRequests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkubCircle.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Route("api/join-requests")]
public class JoinRequestsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public JoinRequestsController(ISender sender, IMapper mapper)
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
    /// Submit a request to join a circle or invite a user to a circle
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(JoinRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateJoinRequest([FromBody] CreateJoinRequestDto request)
    {
        var command = new CreateJoinRequestCommand
        {
            CircleId = request.CircleId,
            RequestedUserId = request.RequestedUserId,
            Email = request.Email,
            Message = request.Message,
            CurrentUserId = GetCurrentUserId()
        };

        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(GetCircleJoinRequests), new { circleId = result.CircleId }, result);
    }

    /// <summary>
    /// Respond to a pending join request (Accept or Reject)
    /// </summary>
    [HttpPut("{id:int}/respond")]
    [ProducesResponseType(typeof(JoinRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RespondJoinRequest(int id, [FromBody] RespondJoinRequestDto request)
    {
        var command = new RespondJoinRequestCommand
        {
            RequestId = id,
            Status = request.Status,
            CurrentUserId = GetCurrentUserId()
        };

        var result = await _sender.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get all join requests for a circle
    /// </summary>
    [HttpGet("circle/{circleId:int}")]
    [ProducesResponseType(typeof(List<JoinRequestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCircleJoinRequests(int circleId)
    {
        var query = new GetCircleJoinRequestsQuery
        {
            CircleId = circleId,
            CurrentUserId = GetCurrentUserId()
        };

        var result = await _sender.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get all join requests sent or received by the current user
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(List<JoinRequestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyJoinRequests()
    {
        var query = new GetUserJoinRequestsQuery
        {
            UserId = GetCurrentUserId()
        };

        var result = await _sender.Send(query);
        return Ok(result);
    }
}
