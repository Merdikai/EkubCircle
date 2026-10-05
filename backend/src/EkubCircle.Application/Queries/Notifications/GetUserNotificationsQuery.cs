using MediatR;
using EkubCircle.Application.DTOs.Notifications;

namespace EkubCircle.Application.Queries.Notifications;

public class GetUserNotificationsQuery : IRequest<List<NotificationDto>>
{
    public int UserId { get; set; }
    public bool? UnreadOnly { get; set; }
}
