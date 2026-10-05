using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.DTOs.JoinRequests;
using EkubCircle.Application.Queries.JoinRequests;

namespace EkubCircle.Application.Handlers.JoinRequests;

public class GetUserJoinRequestsQueryHandler : IRequestHandler<GetUserJoinRequestsQuery, List<JoinRequestDto>>
{
    private readonly IEkubDbContext _context;

    public GetUserJoinRequestsQueryHandler(IEkubDbContext context)
    {
        _context = context;
    }

    public async Task<List<JoinRequestDto>> Handle(GetUserJoinRequestsQuery request, CancellationToken cancellationToken)
    {
        return await _context.JoinRequests
            .AsNoTracking()
            .Include(jr => jr.Circle)
            .Include(jr => jr.RequestedUser)
            .Include(jr => jr.RequestedByUser)
            .Where(jr => jr.RequestedUserId == request.UserId || jr.RequestedByUserId == request.UserId)
            .OrderByDescending(jr => jr.CreatedAt)
            .Select(jr => new JoinRequestDto
            {
                Id = jr.Id,
                CircleId = jr.CircleId,
                CircleName = jr.Circle != null ? jr.Circle.Name : string.Empty,
                RequestedUserId = jr.RequestedUserId,
                RequestedUserName = jr.RequestedUser != null ? jr.RequestedUser.FullName : string.Empty,
                RequestedUserEmail = jr.RequestedUser != null ? jr.RequestedUser.Email : string.Empty,
                RequestedByUserId = jr.RequestedByUserId,
                RequestedByUserName = jr.RequestedByUser != null ? jr.RequestedByUser.FullName : string.Empty,
                Status = jr.Status,
                Message = jr.Message,
                CreatedAt = jr.CreatedAt,
                RespondedAt = jr.RespondedAt
            })
            .ToListAsync(cancellationToken);
    }
}
