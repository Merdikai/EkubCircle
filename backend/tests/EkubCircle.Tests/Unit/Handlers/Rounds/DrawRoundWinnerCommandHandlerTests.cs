using EkubCircle.Application.Commands.Rounds;
using EkubCircle.Application.Handlers.Rounds;
using EkubCircle.Domain.Enums;
using EkubCircle.Tests.Common;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Handlers.Rounds;

public class DrawRoundWinnerCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNoMembersHavePaid_ThrowsInvalidOperationException()
    {
        // Arrange: 2 members, but neither has paid yet
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var u1 = TestDataFactory.CreateUser(id: 1, fullName: "Abebe");
        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, status: CircleStatus.Active);
        var m1 = TestDataFactory.CreateMember(id: 10, circleId: 1, userId: 1, role: CircleRole.Organizer, hasReceived: false);
        var round = TestDataFactory.CreateRound(id: 100, circleId: 1, roundNumber: 1, status: RoundStatus.Open);

        context.Users.Add(u1);
        context.Circles.Add(circle);
        context.CircleMembers.Add(m1);
        context.Rounds.Add(round);
        await context.SaveChangesAsync();

        var handler = new DrawRoundWinnerCommandHandler(context);
        var command = new DrawRoundWinnerCommand(RoundId: 100, RequesterUserId: 1);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert: Unpaid members cannot win the draw
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No eligible candidates found*");
    }

    [Fact]
    public async Task Handle_WhenSingleEligiblePaidMember_SelectsMemberAsWinner()
    {
        // Arrange: 2 members, Member 10 already received in past, Member 20 has paid and not received
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var u1 = TestDataFactory.CreateUser(id: 1, fullName: "Abebe");
        var u2 = TestDataFactory.CreateUser(id: 2, fullName: "Hana");

        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, status: CircleStatus.Active);

        var m1 = TestDataFactory.CreateMember(id: 10, circleId: 1, userId: 1, role: CircleRole.Organizer, hasReceived: true);
        var m2 = TestDataFactory.CreateMember(id: 20, circleId: 1, userId: 2, role: CircleRole.Member, hasReceived: false);

        var round = TestDataFactory.CreateRound(id: 100, circleId: 1, roundNumber: 2, status: RoundStatus.Open);

        // Both members paid for round 2
        var p1 = TestDataFactory.CreatePayment(id: 1, roundId: 100, memberId: 10, amount: 2500m);
        var p2 = TestDataFactory.CreatePayment(id: 2, roundId: 100, memberId: 20, amount: 2500m);

        context.Users.AddRange(u1, u2);
        context.Circles.Add(circle);
        context.CircleMembers.AddRange(m1, m2);
        context.Rounds.Add(round);
        context.Payments.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var handler = new DrawRoundWinnerCommandHandler(context);
        var command = new DrawRoundWinnerCommand(RoundId: 100, RequesterUserId: 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert: Exactly member 20 can win (member 10 already won)
        result.Should().NotBeNull();
        result.WinnerMemberId.Should().Be(20);
        result.EligibleCandidatesCount.Should().Be(1);

        var updatedRound = await context.Rounds.FindAsync(100);
        updatedRound!.WinnerMemberId.Should().Be(20);
    }
}
