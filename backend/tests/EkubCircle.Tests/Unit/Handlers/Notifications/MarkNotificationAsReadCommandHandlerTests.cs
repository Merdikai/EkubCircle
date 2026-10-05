using EkubCircle.Application.Commands.Notifications;
using EkubCircle.Application.Exceptions;
using EkubCircle.Application.Handlers.Notifications;
using EkubCircle.Domain.Entities;
using EkubCircle.Tests.Common;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Handlers.Notifications;

public class MarkNotificationAsReadCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenValidNotification_MarksReadAndStampsTimestamp()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var user = TestDataFactory.CreateUser(id: 5);
        var notification = new Notification
        {
            Id = 1,
            UserId = 5,
            Title = "Round Winner",
            Message = "You won the pot!",
            Type = "WinnerAnnouncement",
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        context.Users.Add(user);
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        var handler = new MarkNotificationAsReadCommandHandler(context);
        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = 1,
            UserId = 5
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var reloaded = await context.Notifications.FindAsync(1);
        reloaded!.IsRead.Should().BeTrue();
        reloaded.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenNotificationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var handler = new MarkNotificationAsReadCommandHandler(context);
        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = 999,
            UserId = 5
        };

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
