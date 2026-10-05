using MediatR;
using EkubCircle.Application.DTOs.Auth;

namespace EkubCircle.Application.Commands.Auth;

public record LoginUserCommand(string Email, string Password) : IRequest<AuthResponseDto>;
