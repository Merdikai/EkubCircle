using EkubCircle.Application.Commands.Auth;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.Handlers.Auth;
using EkubCircle.Domain.Entities;
using EkubCircle.Domain.Enums;
using EkubCircle.Tests.Common;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Handlers.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();

    [Fact]
    public async Task Handle_WhenEmailNotRegistered_CreatesUserAndReturnsToken()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        _passwordHasher.HashPassword("Secret123!").Returns("hashedPassword123");
        _tokenService.GenerateToken(Arg.Any<User>())
            .Returns(("jwt-mock-token-xyz", DateTime.UtcNow.AddDays(7)));

        var handler = new RegisterUserCommandHandler(context, _passwordHasher, _tokenService);
        var command = new RegisterUserCommand(
            FullName: "Abebe Bikila",
            Email: "abebe@ekub.local",
            Password: "Secret123!",
            Role: UserRole.Organizer
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("jwt-mock-token-xyz");
        result.User.Email.Should().Be("abebe@ekub.local");
        result.User.FullName.Should().Be("Abebe Bikila");

        // Verify password hasher was called with plain password
        _passwordHasher.Received(1).HashPassword("Secret123!");
        _tokenService.Received(1).GenerateToken(Arg.Is<User>(u => u.Email == "abebe@ekub.local"));
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var existingUser = TestDataFactory.CreateUser(email: "existing@ekub.local");
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var handler = new RegisterUserCommandHandler(context, _passwordHasher, _tokenService);
        var command = new RegisterUserCommand(
            FullName: "Another Person",
            Email: "EXISTING@ekub.local", // Test case insensitivity
            Password: "Password123!",
            Role: UserRole.Member
        );

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");

        _passwordHasher.DidNotReceive().HashPassword(Arg.Any<string>());
        _tokenService.DidNotReceive().GenerateToken(Arg.Any<User>());
    }
}
