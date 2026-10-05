using MediatR;
using EkubCircle.Application.DTOs.Rounds;

namespace EkubCircle.Application.Commands.Rounds;

public record ExecutePayoutCommand(
    int RoundId,
    int RequesterUserId
) : IRequest<PayoutResultDto>;
