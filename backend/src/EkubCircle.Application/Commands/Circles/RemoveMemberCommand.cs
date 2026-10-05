using MediatR;

namespace EkubCircle.Application.Commands.Circles;

public record RemoveMemberCommand(
    int CircleId,
    int RequesterUserId,
    int MemberId
) : IRequest<Unit>;
