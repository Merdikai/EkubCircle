using BCrypt.Net;
using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Auth;
using EkubCircle.API.Models;
using EkubCircle.API.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Commands.Auth;

public record LoginUserCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly EkubDbContext _context;
    private readonly ITokenService _tokenService;

    public LoginUserCommandHandler(EkubDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

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
