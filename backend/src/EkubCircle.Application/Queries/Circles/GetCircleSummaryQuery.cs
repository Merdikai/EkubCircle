using MediatR;
using EkubCircle.Application.DTOs.Circles;

namespace EkubCircle.Application.Queries.Circles;

public record GetCircleSummaryQuery(
    int CircleId,
    int UserId
) : IRequest<CircleSummaryDto>;
