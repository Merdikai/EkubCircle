using EkubCircle.Application.Commands.Rounds;
using EkubCircle.Application.Handlers.Rounds;
using EkubCircle.Domain.Enums;
using EkubCircle.Tests.Common;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Handlers.Rounds;

public class ExecutePayoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenNotAllMembersContributed_ThrowsInvalidOperationException_Rule4Gate()
    {
        // Arrange: 3 members in circle, but only 2 have contributed for Round 1
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var u1 = TestDataFactory.CreateUser(id: 1, fullName: "Abebe");
        var u2 = TestDataFactory.CreateUser(id: 2, fullName: "Hana");
        var u3 = TestDataFactory.CreateUser(id: 3, fullName: "Dawit");

        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, contributionAmount: 2500m, status: CircleStatus.Active);

        var m1 = TestDataFactory.CreateMember(id: 10, circleId: 1, userId: 1, memberOrder: 1, role: CircleRole.Organizer);
        var m2 = TestDataFactory.CreateMember(id: 20, circleId: 1, userId: 2, memberOrder: 2, role: CircleRole.Member);
        var m3 = TestDataFactory.CreateMember(id: 30, circleId: 1, userId: 3, memberOrder: 3, role: CircleRole.Member);

        var round1 = TestDataFactory.CreateRound(id: 100, circleId: 1, roundNumber: 1, receiverMemberId: 10, status: RoundStatus.Open);

        // Payments: Only m1 and m2 paid. m3 has NOT paid.
        var p1 = TestDataFactory.CreatePayment(id: 1, roundId: 100, memberId: 10, amount: 2500m);
        var p2 = TestDataFactory.CreatePayment(id: 2, roundId: 100, memberId: 20, amount: 2500m);

        context.Users.AddRange(u1, u2, u3);
        context.Circles.Add(circle);
        context.CircleMembers.AddRange(m1, m2, m3);
        context.Rounds.Add(round1);
        context.Payments.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var handler = new ExecutePayoutCommandHandler(context);
        var command = new ExecutePayoutCommand(RoundId: 100, RequesterUserId: 1);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert: Rule 4 - 100% Contribution Gate blocks payout
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only 2 of 3 members have contributed*");

        // Verify round status remains Open and pot was not disbursed
        var reloadedRound = await context.Rounds.FindAsync(100);
        reloadedRound!.Status.Should().Be(RoundStatus.Open);
        reloadedRound.PaidOutAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenReceiverAlreadyReceived_ThrowsInvalidOperationException_Rule5SingleReceipt()
    {
        // Arrange: 2 members, but receiver has HasReceived == true
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var u1 = TestDataFactory.CreateUser(id: 1, fullName: "Abebe");
        var u2 = TestDataFactory.CreateUser(id: 2, fullName: "Hana");

        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, contributionAmount: 1000m, status: CircleStatus.Active);

        var m1 = TestDataFactory.CreateMember(id: 10, circleId: 1, userId: 1, memberOrder: 1, role: CircleRole.Organizer, hasReceived: true);
        var m2 = TestDataFactory.CreateMember(id: 20, circleId: 1, userId: 2, memberOrder: 2, role: CircleRole.Member, hasReceived: false);

        var round = TestDataFactory.CreateRound(id: 100, circleId: 1, roundNumber: 1, receiverMemberId: 10, status: RoundStatus.Open);

        // Both members paid
        var p1 = TestDataFactory.CreatePayment(id: 1, roundId: 100, memberId: 10, amount: 1000m);
        var p2 = TestDataFactory.CreatePayment(id: 2, roundId: 100, memberId: 20, amount: 1000m);

        context.Users.AddRange(u1, u2);
        context.Circles.Add(circle);
        context.CircleMembers.AddRange(m1, m2);
        context.Rounds.Add(round);
        context.Payments.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var handler = new ExecutePayoutCommandHandler(context);
        var command = new ExecutePayoutCommand(RoundId: 100, RequesterUserId: 1);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert: Rule 5 - Single Pot Receipt
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already received a pot payout*");
    }

    [Fact]
    public async Task Handle_When100PercentPaid_ReleasesPotMarksRoundPaidOutAndAdvancesNextRound()
    {
        // Arrange: 2 members, Round 1 (Open) and Round 2 (Pending), both paid for Round 1
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var u1 = TestDataFactory.CreateUser(id: 1, fullName: "Abebe");
        var u2 = TestDataFactory.CreateUser(id: 2, fullName: "Hana");

        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, contributionAmount: 3000m, status: CircleStatus.Active);

        var m1 = TestDataFactory.CreateMember(id: 10, circleId: 1, userId: 1, memberOrder: 1, role: CircleRole.Organizer, hasReceived: false);
        var m2 = TestDataFactory.CreateMember(id: 20, circleId: 1, userId: 2, memberOrder: 2, role: CircleRole.Member, hasReceived: false);

        var round1 = TestDataFactory.CreateRound(id: 100, circleId: 1, roundNumber: 1, receiverMemberId: 10, status: RoundStatus.Open);
        var round2 = TestDataFactory.CreateRound(id: 101, circleId: 1, roundNumber: 2, receiverMemberId: 20, status: RoundStatus.Pending);

        var p1 = TestDataFactory.CreatePayment(id: 1, roundId: 100, memberId: 10, amount: 3000m);
        var p2 = TestDataFactory.CreatePayment(id: 2, roundId: 100, memberId: 20, amount: 3000m);

        context.Users.AddRange(u1, u2);
        context.Circles.Add(circle);
        context.CircleMembers.AddRange(m1, m2);
        context.Rounds.AddRange(round1, round2);
        context.Payments.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var handler = new ExecutePayoutCommandHandler(context);
        var command = new ExecutePayoutCommand(RoundId: 100, RequesterUserId: 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert: 100% paid path releases full pot
        result.Should().NotBeNull();
        result.PotAmount.Should().Be(6000m); // 2 * 3000m
        result.ReceiverMemberId.Should().Be(10);
        result.RoundStatus.Should().Be(RoundStatus.PaidOut);
        result.NextRoundNumber.Should().Be(2);

        // Verify receiver is marked as received
        var updatedReceiver = await context.CircleMembers.FindAsync(10);
        updatedReceiver!.HasReceived.Should().BeTrue();

        // Verify payout payment entity was created
        var payoutPayment = context.Payments.FirstOrDefault(p => p.PaymentType == PaymentType.Payout);
        payoutPayment.Should().NotBeNull();
        payoutPayment!.Amount.Should().Be(6000m);
        payoutPayment.MemberId.Should().Be(10);

        // Verify Round 2 was advanced to Open
        var updatedRound2 = await context.Rounds.FindAsync(101);
        updatedRound2!.Status.Should().Be(RoundStatus.Open);
    }
}
