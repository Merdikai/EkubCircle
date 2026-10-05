using MediatR;
using EkubCircle.Application.DTOs.Rounds;

namespace EkubCircle.Application.Queries.Rounds;

public record GetCurrentRoundQuery(int CircleId, int UserId) : IRequest<CurrentRoundDto>;
