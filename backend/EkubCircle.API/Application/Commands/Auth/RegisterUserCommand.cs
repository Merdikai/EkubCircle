using BCrypt.Net;
using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Auth;
using EkubCircle.API.Models;
using EkubCircle.API.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Commands.Auth;

public record RegisterUserCommand(string FullName, string Email, string Password) : IRequest<AuthResponseDto>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly EkubDbContext _context;
    private readonly ITokenService _tokenService;

    public RegisterUserCommandHandler(EkubDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = UserRole.User,
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
