using MediatR;
using EkubCircle.Application.DTOs.JoinRequests;

namespace EkubCircle.Application.Commands.JoinRequests;

public class RespondJoinRequestCommand : IRequest<JoinRequestDto>
{
    public int RequestId { get; set; }
    public string Status { get; set; } = "Accepted"; // "Accepted" or "Rejected"
    public int CurrentUserId { get; set; }
}
