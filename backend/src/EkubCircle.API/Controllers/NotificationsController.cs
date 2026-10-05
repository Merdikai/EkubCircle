using System.Security.Claims;
using AutoMapper;
using EkubCircle.Application.Commands.Notifications;
using EkubCircle.Application.DTOs.Notifications;
using EkubCircle.Application.Queries.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkubCircle.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public NotificationsController(ISender sender, IMapper mapper)
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
    /// Get current user's notifications
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] bool? unreadOnly)
    {
        var query = new GetUserNotificationsQuery
        {
            UserId = GetCurrentUserId(),
            UnreadOnly = unreadOnly
        };

        var result = await _sender.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    [HttpPut("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = id,
            UserId = GetCurrentUserId()
        };

        var result = await _sender.Send(command);
        return Ok(new { success = result });
    }
}
