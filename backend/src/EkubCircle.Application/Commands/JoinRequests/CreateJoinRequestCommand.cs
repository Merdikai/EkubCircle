using MediatR;
using EkubCircle.Application.DTOs.JoinRequests;

namespace EkubCircle.Application.Commands.JoinRequests;

public class CreateJoinRequestCommand : IRequest<JoinRequestDto>
{
    public int CircleId { get; set; }
    public int? RequestedUserId { get; set; }
    public string? Email { get; set; }
    public string? Message { get; set; }
    public int CurrentUserId { get; set; }
}
