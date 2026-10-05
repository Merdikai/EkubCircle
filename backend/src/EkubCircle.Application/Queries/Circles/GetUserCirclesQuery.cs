using MediatR;
using EkubCircle.Application.DTOs.Circles;

namespace EkubCircle.Application.Queries.Circles;

public record GetUserCirclesQuery(int UserId, string? Status = null) : IRequest<List<CircleDto>>;
