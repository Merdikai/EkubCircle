using MediatR;
using EkubCircle.Application.DTOs.Circles;

namespace EkubCircle.Application.Commands.Circles;

public record AddMemberCommand(
    int CircleId,
    int RequesterUserId,
    string MemberEmail
) : IRequest<CircleMemberDto>;
