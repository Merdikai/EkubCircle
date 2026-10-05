using EkubCircle.Application.Commands.Payments;
using EkubCircle.Application.Handlers.Payments;
using EkubCircle.Domain.Enums;
using EkubCircle.Tests.Common;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Handlers.Payments;

public class RecordPaymentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenMemberAlreadyContributedForRound_ThrowsInvalidOperationException_Rule7AntiDuplicate()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var u1 = TestDataFactory.CreateUser(id: 1, fullName: "Abebe");
        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, contributionAmount: 2500m, status: CircleStatus.Active);
        var member = TestDataFactory.CreateMember(id: 10, circleId: 1, userId: 1, role: CircleRole.Organizer);
        var round = TestDataFactory.CreateRound(id: 100, circleId: 1, roundNumber: 1, status: RoundStatus.Open);

        // Pre-existing contribution
        var existingPayment = TestDataFactory.CreatePayment(id: 1, roundId: 100, memberId: 10, amount: 2500m);

        context.Users.Add(u1);
        context.Circles.Add(circle);
        context.CircleMembers.Add(member);
        context.Rounds.Add(round);
        context.Payments.Add(existingPayment);
        await context.SaveChangesAsync();

        var handler = new RecordPaymentCommandHandler(context);
        var command = new RecordPaymentCommand(
            RoundId: 100,
            MemberId: 10,
            Amount: 2500m,
            PaymentMethod: "Cash",
            Notes: null,
            IsLate: false,
            RecordedByUserId: 1
        );

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert: Rule 7 - Duplicate contribution strictly rejected
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already made a contribution for this round*");
    }

    [Fact]
    public async Task Handle_WhenMemberHasAlreadyReceivedPotInPast_StillAllowsPayment_Rule6PostReceipt()
    {
        // Arrange: Member 10 won pot in Round 1 (HasReceived == true), now paying in Round 2
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var u1 = TestDataFactory.CreateUser(id: 1, fullName: "Abebe");
        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, contributionAmount: 2000m, status: CircleStatus.Active);

        // Member has already received pot
        var member = TestDataFactory.CreateMember(id: 10, circleId: 1, userId: 1, role: CircleRole.Organizer, hasReceived: true);
        var round2 = TestDataFactory.CreateRound(id: 102, circleId: 1, roundNumber: 2, status: RoundStatus.Open);

        context.Users.Add(u1);
        context.Circles.Add(circle);
        context.CircleMembers.Add(member);
        context.Rounds.Add(round2);
        await context.SaveChangesAsync();

        var handler = new RecordPaymentCommandHandler(context);
        var command = new RecordPaymentCommand(
            RoundId: 102,
            MemberId: 10,
            Amount: 2000m,
            PaymentMethod: "Telebirr",
            Notes: "Round 2 post-win contribution",
            IsLate: false,
            RecordedByUserId: 1
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert: Rule 6 - Post-receipt contributions remain valid and mandatory
        result.Should().NotBeNull();
        result.Amount.Should().Be(2000m);
        result.PaymentType.Should().Be(PaymentType.Contribution);
        result.MemberId.Should().Be(10);

        var savedPayment = context.Payments.FirstOrDefault(p => p.RoundId == 102 && p.CircleMemberId == 10);
        savedPayment.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenAmountMismatchesCircleContribution_ThrowsArgumentException()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var u1 = TestDataFactory.CreateUser(id: 1);
        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, contributionAmount: 2500m, status: CircleStatus.Active);
        var member = TestDataFactory.CreateMember(id: 10, circleId: 1, userId: 1, role: CircleRole.Organizer);
        var round = TestDataFactory.CreateRound(id: 100, circleId: 1, roundNumber: 1, status: RoundStatus.Open);

        context.Users.Add(u1);
        context.Circles.Add(circle);
        context.CircleMembers.Add(member);
        context.Rounds.Add(round);
        await context.SaveChangesAsync();

        var handler = new RecordPaymentCommandHandler(context);
        var command = new RecordPaymentCommand(
            RoundId: 100,
            MemberId: 10,
            Amount: 1500m, // Mismatched: expected 2500m
            PaymentMethod: "Cash",
            Notes: null,
            IsLate: false,
            RecordedByUserId: 1
        );

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*must equal the fixed contribution amount*");
    }
}
