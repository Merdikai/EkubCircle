using EkubCircle.Application.Commands.Auth;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.Handlers.Auth;
using EkubCircle.Domain.Entities;
using EkubCircle.Tests.Common;
using EkubCircle.Tests.Common.Fixtures;

namespace EkubCircle.Tests.Unit.Handlers.Auth;

public class LoginUserCommandHandlerTests
{
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var user = TestDataFactory.CreateUser(email: "valid@ekub.local");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _passwordHasher.VerifyPassword("Password123!", user.PasswordHash).Returns(true);
        _tokenService.GenerateToken(Arg.Is<User>(u => u.Id == user.Id))
            .Returns(("valid-jwt-token", DateTime.UtcNow.AddDays(7)));

        var handler = new LoginUserCommandHandler(context, _passwordHasher, _tokenService);
        var command = new LoginUserCommand(Email: "valid@ekub.local", Password: "Password123!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("valid-jwt-token");
        result.User.Email.Should().Be("valid@ekub.local");

        _passwordHasher.Received(1).VerifyPassword("Password123!", user.PasswordHash);
        _tokenService.Received(1).GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var user = TestDataFactory.CreateUser(email: "valid@ekub.local");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _passwordHasher.VerifyPassword("WrongPassword!", user.PasswordHash).Returns(false);

        var handler = new LoginUserCommandHandler(context, _passwordHasher, _tokenService);
        var command = new LoginUserCommand(Email: "valid@ekub.local", Password: "WrongPassword!");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");

        _tokenService.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        await using var _ = connection;
        await using var __ = context;

        var handler = new LoginUserCommandHandler(context, _passwordHasher, _tokenService);
        var command = new LoginUserCommand(Email: "nonexistent@ekub.local", Password: "Password123!");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _passwordHasher.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
        _tokenService.DidNotReceive().GenerateToken(Arg.Any<User>());
    }
}
