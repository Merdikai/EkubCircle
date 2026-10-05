using EkubCircle.Domain.Entities;
using EkubCircle.Domain.Enums;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Domain;

public class CircleEntityTests
{
    [Fact]
    public void Constructor_DefaultState_InitializesWithFormingStatus()
    {
        // Arrange & Act
        var circle = new Circle
        {
            Name = "Bole Tech Circle",
            ContributionAmount = 1000m,
            MeetingLabel = "Weekly",
            CreatedByUserId = 1
        };

        // Assert
        circle.Status.Should().Be(CircleStatus.Draft);
        circle.StartedAt.Should().BeNull();
        circle.CompletedAt.Should().BeNull();
        circle.Members.Should().BeEmpty();
        circle.Rounds.Should().BeEmpty();
    }

    [Theory]
    [InlineData(100.00)]
    [InlineData(2500.50)]
    [InlineData(50000.00)]
    public void ContributionAmount_ValidNumericValues_StoresAccurately(decimal amount)
    {
        // Arrange & Act
        var circle = TestDataFactory.CreateCircle(contributionAmount: amount);

        // Assert
        circle.ContributionAmount.Should().Be(amount);
    }

    [Fact]
    public void StatusTransition_WhenStarted_ReflectsActive()
    {
        // Arrange
        var circle = TestDataFactory.CreateCircle();
        var startTime = DateTime.UtcNow;

        // Act
        circle.Status = CircleStatus.Active;
        circle.StartedAt = startTime;

        // Assert
        circle.Status.Should().Be(CircleStatus.Active);
        circle.StartedAt.Should().Be(startTime);
    }

    [Fact]
    public void StatusTransition_WhenCompleted_ReflectsCompleted()
    {
        // Arrange
        var circle = TestDataFactory.CreateCircle(status: CircleStatus.Active);
        var completedTime = DateTime.UtcNow;

        // Act
        circle.Status = CircleStatus.Completed;
        circle.CompletedAt = completedTime;

        // Assert
        circle.Status.Should().Be(CircleStatus.Completed);
        circle.CompletedAt.Should().Be(completedTime);
    }
}
