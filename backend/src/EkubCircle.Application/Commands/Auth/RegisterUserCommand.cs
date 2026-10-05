using MediatR;
using EkubCircle.Application.DTOs.Auth;

namespace EkubCircle.Application.Commands.Auth;

public record RegisterUserCommand(string FullName, string Email, string Password, string Role = "User") : IRequest<AuthResponseDto>;
