using MediatR;

namespace EkubCircle.Application.Commands.Notifications;

public class MarkNotificationAsReadCommand : IRequest<bool>
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
}
