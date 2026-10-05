using EkubCircle.Application.Commands.JoinRequests;
using EkubCircle.Application.Handlers.JoinRequests;
using EkubCircle.Domain.Enums;
using EkubCircle.Domain.Exceptions;
using EkubCircle.Tests.Common;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Handlers.JoinRequests;

public class CreateJoinRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCircleIsActive_ThrowsEkubRuleViolationException()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var user = TestDataFactory.CreateUser(id: 1);
        var activeCircle = TestDataFactory.CreateCircle(id: 10, status: CircleStatus.Active);

        context.Users.Add(user);
        context.Circles.Add(activeCircle);
        await context.SaveChangesAsync();

        var handler = new CreateJoinRequestCommandHandler(context);
        var command = new CreateJoinRequestCommand
        {
            CircleId = 10,
            CurrentUserId = 1,
            RequestedUserId = null,
            Email = null,
            Message = "Please let me join!"
        };

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EkubRuleViolationException>()
            .WithMessage("*already Active or Completed*");
    }

    [Fact]
    public async Task Handle_WhenCircleAtMaxCapacity_ThrowsEkubRuleViolationException()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var u1 = TestDataFactory.CreateUser(id: 1);
        var u2 = TestDataFactory.CreateUser(id: 2);
        var circle = TestDataFactory.CreateCircle(id: 10, status: CircleStatus.Forming);
        circle.MaxMembers = 1; // Limit capacity to 1

        var m1 = TestDataFactory.CreateMember(id: 1, circleId: 10, userId: 1);

        context.Users.AddRange(u1, u2);
        context.Circles.Add(circle);
        context.CircleMembers.Add(m1);
        await context.SaveChangesAsync();

        var handler = new CreateJoinRequestCommandHandler(context);
        var command = new CreateJoinRequestCommand
        {
            CircleId = 10,
            CurrentUserId = 2,
            RequestedUserId = null,
            Email = null,
            Message = "Joining request"
        };

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EkubRuleViolationException>()
            .WithMessage("*maximum capacity*");
    }
}
