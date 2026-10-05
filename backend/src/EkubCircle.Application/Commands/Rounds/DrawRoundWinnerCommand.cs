using MediatR;
using EkubCircle.Application.DTOs.Rounds;

namespace EkubCircle.Application.Commands.Rounds;

public record DrawRoundWinnerCommand(
    int RoundId,
    int RequesterUserId
) : IRequest<DrawWinnerDto>;
