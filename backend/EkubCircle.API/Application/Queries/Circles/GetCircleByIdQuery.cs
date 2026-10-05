using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Circles;
using EkubCircle.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Queries.Circles;

public record GetCircleByIdQuery(int CircleId, int UserId) : IRequest<CircleDetailDto>;

public class GetCircleByIdQueryHandler : IRequestHandler<GetCircleByIdQuery, CircleDetailDto>
{
    private readonly EkubDbContext _context;

    public GetCircleByIdQueryHandler(EkubDbContext context)
    {
        _context = context;
    }

    public async Task<CircleDetailDto> Handle(GetCircleByIdQuery request, CancellationToken cancellationToken)
    {
        var circle = await _context.Circles
            .Include(c => c.CreatedByUser)
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .Include(c => c.Rounds)
            .FirstOrDefaultAsync(c => c.Id == request.CircleId, cancellationToken)
            ?? throw new KeyNotFoundException("Circle not found.");

        if (!circle.Members.Any(m => m.UserId == request.UserId))
        {
            throw new UnauthorizedAccessException("You are not a member of this circle.");
        }

        var currentRound = circle.Rounds.FirstOrDefault(r => r.Status == RoundStatus.Open);

        return new CircleDetailDto
        {
            Id = circle.Id,
            Name = circle.Name,
            ContributionAmount = circle.ContributionAmount,
            MeetingLabel = circle.MeetingLabel,
            Status = circle.Status,
            CreatedByUserId = circle.CreatedByUserId,
            CreatedByUserName = circle.CreatedByUser?.FullName ?? "Unknown",
            CreatedAt = circle.CreatedAt,
            StartedAt = circle.StartedAt,
            CompletedAt = circle.CompletedAt,
            MemberCount = circle.Members.Count,
            TotalRounds = circle.Rounds.Count,
            CurrentRoundNumber = currentRound?.RoundNumber ?? (circle.Status == CircleStatus.Completed ? circle.Rounds.Count : 0),
            Members = circle.Members
                .OrderBy(m => m.MemberOrder > 0 ? m.MemberOrder : int.MaxValue)
                .ThenBy(m => m.JoinedAt)
                .Select(m => new CircleMemberDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    FullName = m.User?.FullName ?? "Unknown",
                    Email = m.User?.Email ?? "Unknown",
                    MemberOrder = m.MemberOrder,
                    RoleInCircle = m.RoleInCircle,
                    HasReceived = m.HasReceived,
                    JoinedAt = m.JoinedAt
                }).ToList()
        };
    }
}
