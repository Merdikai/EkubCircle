using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Commands.Auth;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.DTOs.Auth;
using EkubCircle.Domain.Entities;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Application.Handlers.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IEkubDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterUserCommandHandler(
        IEkubDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (existingUser)
        {
            throw new InvalidOperationException("A user with this email address already exists.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = string.Equals(request.Role, UserRole.Admin, StringComparison.OrdinalIgnoreCase) ? UserRole.Admin : UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var tokenResult = _tokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = tokenResult.Token,
            ExpiresAt = tokenResult.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };
    }
}
