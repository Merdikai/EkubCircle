using EkubCircle.Domain.Entities;
using EkubCircle.Domain.Enums;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Domain;

public class PaymentEntityTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var payment = TestDataFactory.CreatePayment(
            id: 1,
            roundId: 2,
            memberId: 3,
            amount: 2500m,
            type: PaymentType.Contribution,
            recordedByUserId: 1);

        // Assert
        payment.RoundId.Should().Be(2);
        payment.MemberId.Should().Be(3);
        payment.Amount.Should().Be(2500m);
        payment.PaymentType.Should().Be(PaymentType.Contribution);
        payment.PaymentMethod.Should().Be("Telebirr");
    }

    [Theory]
    [InlineData(PaymentType.Contribution)]
    [InlineData(PaymentType.Payout)]
    public void PaymentType_ValidTypes_MatchesAllowedEnums(string type)
    {
        // Arrange & Act
        var payment = TestDataFactory.CreatePayment(type: type);

        // Assert
        payment.PaymentType.Should().Be(type);
    }
}
