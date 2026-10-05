using MediatR;
using EkubCircle.Application.DTOs.Auth;

namespace EkubCircle.Application.Queries.Auth;

public record GetCurrentUserQuery(int UserId) : IRequest<UserDto>;
