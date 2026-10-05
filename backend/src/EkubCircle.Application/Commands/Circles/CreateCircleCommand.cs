using MediatR;
using EkubCircle.Application.DTOs.Circles;

namespace EkubCircle.Application.Commands.Circles;

public record CreateCircleCommand(
    int UserId,
    string Name,
    decimal ContributionAmount,
    string MeetingLabel = "Weekly"
) : IRequest<CircleDetailDto>;
