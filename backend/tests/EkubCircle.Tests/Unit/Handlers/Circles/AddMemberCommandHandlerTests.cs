using EkubCircle.Application.Commands.Circles;
using EkubCircle.Application.Handlers.Circles;
using EkubCircle.Domain.Enums;
using EkubCircle.Tests.Common;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Handlers.Circles;

public class AddMemberCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCircleIsActive_ThrowsInvalidOperationException_Rule1MemberLock()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var organizer = TestDataFactory.CreateUser(id: 1, email: "organizer@ekub.local");
        var activeCircle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, status: CircleStatus.Active);
        var organizerMember = TestDataFactory.CreateMember(id: 1, circleId: 1, userId: 1, role: CircleRole.Organizer);
        var candidateUser = TestDataFactory.CreateUser(id: 2, email: "candidate@ekub.local");

        context.Users.AddRange(organizer, candidateUser);
        context.Circles.Add(activeCircle);
        context.CircleMembers.Add(organizerMember);
        await context.SaveChangesAsync();

        var handler = new AddMemberCommandHandler(context);
        var command = new AddMemberCommand(CircleId: 1, MemberEmail: "candidate@ekub.local", RequesterUserId: 1);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert: Rule 1 - Member list locked once Active
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Roster is locked once active*");
    }

    [Fact]
    public async Task Handle_WhenRequesterIsNotOrganizer_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var organizer = TestDataFactory.CreateUser(id: 1, email: "organizer@ekub.local");
        var regularUser = TestDataFactory.CreateUser(id: 2, email: "regular@ekub.local");
        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, status: CircleStatus.Forming);

        context.Users.AddRange(organizer, regularUser);
        context.Circles.Add(circle);
        await context.SaveChangesAsync();

        var handler = new AddMemberCommandHandler(context);
        var command = new AddMemberCommand(CircleId: 1, MemberEmail: "candidate@ekub.local", RequesterUserId: 2);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Only the circle organizer can add members*");
    }

    [Fact]
    public async Task Handle_WhenCandidateAlreadyMember_ThrowsInvalidOperationException()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var organizer = TestDataFactory.CreateUser(id: 1, email: "organizer@ekub.local");
        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, status: CircleStatus.Forming);
        var member = TestDataFactory.CreateMember(id: 1, circleId: 1, userId: 1, role: CircleRole.Organizer);

        context.Users.Add(organizer);
        context.Circles.Add(circle);
        context.CircleMembers.Add(member);
        await context.SaveChangesAsync();

        var handler = new AddMemberCommandHandler(context);
        var command = new AddMemberCommand(CircleId: 1, MemberEmail: "organizer@ekub.local", RequesterUserId: 1);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already a member*");
    }

    [Fact]
    public async Task Handle_WhenValid_AddsMemberAndAssignsNextOrder()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var organizer = TestDataFactory.CreateUser(id: 1, email: "organizer@ekub.local");
        var candidate = TestDataFactory.CreateUser(id: 2, fullName: "Hana G.", email: "hana@ekub.local");
        var circle = TestDataFactory.CreateCircle(id: 1, creatorUserId: 1, status: CircleStatus.Forming);
        var member1 = TestDataFactory.CreateMember(id: 1, circleId: 1, userId: 1, memberOrder: 1, role: CircleRole.Organizer);

        context.Users.AddRange(organizer, candidate);
        context.Circles.Add(circle);
        context.CircleMembers.Add(member1);
        await context.SaveChangesAsync();

        var handler = new AddMemberCommandHandler(context);
        var command = new AddMemberCommand(CircleId: 1, MemberEmail: "hana@ekub.local", RequesterUserId: 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(2);
        result.FullName.Should().Be("Hana G.");
        result.MemberOrder.Should().Be(2); // Order must be sequential
        result.RoleInCircle.Should().Be(CircleRole.Member);
        result.HasReceived.Should().BeFalse();
    }
}
