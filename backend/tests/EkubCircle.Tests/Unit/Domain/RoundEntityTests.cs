using EkubCircle.Domain.Entities;
using EkubCircle.Domain.Enums;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Domain;

public class RoundEntityTests
{
    [Fact]
    public void Constructor_DefaultState_InitializesWithOpenStatus()
    {
        // Arrange & Act
        var round = new Round
        {
            CircleId = 1,
            RoundNumber = 1,
            ReceiverMemberId = 5,
            PotAmount = 10000m
        };

        // Assert
        round.Status.Should().Be(RoundStatus.Pending);
        round.PaidOutAt.Should().BeNull();
        round.Payments.Should().BeEmpty();
    }

    [Theory]
    [InlineData(2500, 4, 10000)]
    [InlineData(1000, 10, 10000)]
    [InlineData(500, 3, 1500)]
    public void PotCalculation_ContributionMultipliedByMembers_MatchesExpected(decimal contribution, int memberCount, decimal expectedPot)
    {
        // Arrange & Act
        var calculatedPot = contribution * memberCount;

        // Assert
        calculatedPot.Should().Be(expectedPot);
    }

    [Fact]
    public void PayoutTransition_WhenPaidOut_SetsStatusAndTimestamp()
    {
        // Arrange
        var round = TestDataFactory.CreateRound();
        var payoutTime = DateTime.UtcNow;

        // Act
        round.Status = RoundStatus.PaidOut;
        round.PaidOutAt = payoutTime;

        // Assert
        round.Status.Should().Be(RoundStatus.PaidOut);
        round.PaidOutAt.Should().Be(payoutTime);
    }
}
