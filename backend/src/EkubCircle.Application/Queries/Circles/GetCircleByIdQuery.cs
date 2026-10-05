using MediatR;
using EkubCircle.Application.DTOs.Circles;

namespace EkubCircle.Application.Queries.Circles;

public record GetCircleByIdQuery(int CircleId, int UserId) : IRequest<CircleDetailDto>;
