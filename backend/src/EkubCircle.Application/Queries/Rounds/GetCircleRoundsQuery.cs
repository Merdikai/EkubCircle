using MediatR;
using EkubCircle.Application.DTOs.Rounds;

namespace EkubCircle.Application.Queries.Rounds;

public record GetCircleRoundsQuery(int CircleId, int UserId) : IRequest<List<RoundSummaryDto>>;
