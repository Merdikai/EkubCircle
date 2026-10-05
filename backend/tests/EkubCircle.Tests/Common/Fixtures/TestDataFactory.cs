using EkubCircle.Domain.Entities;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Tests.Common.Fixtures;

public static class TestDataFactory
{
    public static User CreateUser(int id = 1, string? fullName = null, string? email = null, string role = "Organizer")
    {
        return new User
        {
            Id = id,
            FullName = fullName ?? $"User {id}",
            Email = email ?? $"user{id}@ekub.local",
            PasswordHash = "$2a$11$testHashValueHereForTestingOnly",
            Role = role,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    public static Circle CreateCircle(int id = 1, int creatorUserId = 1, string name = "Bole Tech Savings", decimal contributionAmount = 2500m, string status = CircleStatus.Forming)
    {
        return new Circle
        {
            Id = id,
            Name = name,
            ContributionAmount = contributionAmount,
            MeetingLabel = "Weekly",
            Status = status,
            CreatedByUserId = creatorUserId,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    public static CircleMember CreateMember(int id = 1, int circleId = 1, int userId = 1, int memberOrder = 1, string role = "Organizer", bool hasReceived = false)
    {
        return new CircleMember
        {
            Id = id,
            CircleId = circleId,
            UserId = userId,
            MemberOrder = memberOrder,
            RoleInCircle = role,
            HasReceived = hasReceived,
            JoinedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    public static Round CreateRound(int id = 1, int circleId = 1, int roundNumber = 1, int? receiverMemberId = null, decimal potAmount = 10000m, string status = RoundStatus.Open)
    {
        return new Round
        {
            Id = id,
            CircleId = circleId,
            RoundNumber = roundNumber,
            Status = status,
            WinnerMemberId = receiverMemberId,
            PotAmount = potAmount
        };
    }

    public static Payment CreatePayment(int id = 1, int roundId = 1, int memberId = 1, decimal amount = 2500m, string type = PaymentType.Contribution, int recordedByUserId = 1)
    {
        return new Payment
        {
            Id = id,
            RoundId = roundId,
            MemberId = memberId,
            Amount = amount,
            PaymentType = type,
            PaymentMethod = "Telebirr",
            PaidAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            RecordedByUserId = recordedByUserId
        };
    }
}
