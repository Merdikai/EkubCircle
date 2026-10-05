using MediatR;
using EkubCircle.Application.DTOs.Circles;

namespace EkubCircle.Application.Commands.Circles;

public record StartCircleCommand(
    int CircleId,
    int RequesterUserId
) : IRequest<CircleDetailDto>;
