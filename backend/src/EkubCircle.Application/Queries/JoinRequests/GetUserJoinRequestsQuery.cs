using MediatR;
using EkubCircle.Application.DTOs.JoinRequests;

namespace EkubCircle.Application.Queries.JoinRequests;

public class GetUserJoinRequestsQuery : IRequest<List<JoinRequestDto>>
{
    public int UserId { get; set; }
}
