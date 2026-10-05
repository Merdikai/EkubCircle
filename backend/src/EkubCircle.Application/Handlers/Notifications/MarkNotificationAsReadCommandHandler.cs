using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Commands.Notifications;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.Exceptions;
using EkubCircle.Domain.Exceptions;

namespace EkubCircle.Application.Handlers.Notifications;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly IEkubDbContext _context;

    public MarkNotificationAsReadCommandHandler(IEkubDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, cancellationToken);

        if (notification == null)
        {
            throw new NotFoundException($"Notification with ID {request.NotificationId} not found.");
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
