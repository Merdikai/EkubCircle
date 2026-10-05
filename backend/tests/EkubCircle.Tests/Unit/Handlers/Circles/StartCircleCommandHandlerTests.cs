using EkubCircle.Application.Commands.Circles;
using EkubCircle.Application.Handlers.Circles;
using EkubCircle.Domain.Enums;
using EkubCircle.Tests.Common;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Handlers.Circles;

public class StartCircleCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithLessThanTwoMembers_ThrowsInvalidOperationException()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var organizer = TestDataFactory.CreateUser(id: 1, email: "organizer@ekub.local");
        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, status: CircleStatus.Forming);
        var singleMember = TestDataFactory.CreateMember(id: 1, circleId: 1, userId: 1, role: CircleRole.Organizer);

        context.Users.Add(organizer);
        context.Circles.Add(circle);
        context.CircleMembers.Add(singleMember);
        await context.SaveChangesAsync();

        var handler = new StartCircleCommandHandler(context);
        var command = new StartCircleCommand(CircleId: 1, RequesterUserId: 1);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*must have at least 2 members*");
    }

    [Fact]
    public async Task Handle_WhenValid_AssignsDeterministicOrdersAndCreatesNRounds_Rules2And3()
    {
        // Arrange: 3 members in a forming circle
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var u1 = TestDataFactory.CreateUser(id: 1, fullName: "Abebe");
        var u2 = TestDataFactory.CreateUser(id: 2, fullName: "Hana");
        var u3 = TestDataFactory.CreateUser(id: 3, fullName: "Dawit");
        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, status: CircleStatus.Forming);

        var m1 = TestDataFactory.CreateMember(id: 10, circleId: 1, userId: 1, memberOrder: 1, role: CircleRole.Organizer);
        var m2 = TestDataFactory.CreateMember(id: 20, circleId: 1, userId: 2, memberOrder: 2, role: CircleRole.Member);
        var m3 = TestDataFactory.CreateMember(id: 30, circleId: 1, userId: 3, memberOrder: 3, role: CircleRole.Member);

        context.Users.AddRange(u1, u2, u3);
        context.Circles.Add(circle);
        context.CircleMembers.AddRange(m1, m2, m3);
        await context.SaveChangesAsync();

        var handler = new StartCircleCommandHandler(context);
        var command = new StartCircleCommand(CircleId: 1, RequesterUserId: 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(CircleStatus.Active);
        result.StartedAt.Should().NotBeNull();
        result.TotalRounds.Should().Be(3);
        result.CurrentRoundNumber.Should().Be(1);

        // Verify Rule 2 & 3: Database has exactly 3 rounds, each with the designated receiver matching the member order
        var rounds = context.Rounds.Where(r => r.CircleId == 1).OrderBy(r => r.RoundNumber).ToList();
        rounds.Should().HaveCount(3);

        // Round 1 is Open, Receiver is Member 10 (order 1)
        rounds[0].RoundNumber.Should().Be(1);
        rounds[0].Status.Should().Be(RoundStatus.Open);
        rounds[0].WinnerMemberId.Should().Be(10);

        // Round 2 is Pending, Receiver is Member 20 (order 2)
        rounds[1].RoundNumber.Should().Be(2);
        rounds[1].Status.Should().Be(RoundStatus.Pending);
        rounds[1].WinnerMemberId.Should().Be(20);

        // Round 3 is Pending, Receiver is Member 30 (order 3)
        rounds[2].RoundNumber.Should().Be(3);
        rounds[2].Status.Should().Be(RoundStatus.Pending);
        rounds[2].WinnerMemberId.Should().Be(30);

        // Ensure distinct receivers across all rounds
        rounds.Select(r => r.WinnerMemberId).Distinct().Should().HaveCount(3);
    }
}
