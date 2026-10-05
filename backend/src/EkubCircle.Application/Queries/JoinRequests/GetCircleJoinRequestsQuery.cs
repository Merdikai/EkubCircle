using MediatR;
using EkubCircle.Application.DTOs.JoinRequests;

namespace EkubCircle.Application.Queries.JoinRequests;

public class GetCircleJoinRequestsQuery : IRequest<List<JoinRequestDto>>
{
    public int CircleId { get; set; }
    public int CurrentUserId { get; set; }
}
