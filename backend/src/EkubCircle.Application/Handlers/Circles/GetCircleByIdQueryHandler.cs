using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.DTOs.Circles;
using EkubCircle.Application.Queries.Circles;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Application.Handlers.Circles;

public class GetCircleByIdQueryHandler : IRequestHandler<GetCircleByIdQuery, CircleDetailDto>
{
    private readonly IEkubDbContext _context;

    public GetCircleByIdQueryHandler(IEkubDbContext context)
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
            .FirstOrDefaultAsync(c => c.Id == request.CircleId, cancellationToken);

        if (circle == null)
        {
            throw new KeyNotFoundException($"Circle with ID {request.CircleId} was not found.");
        }

        var isMember = circle.Members.Any(m => m.UserId == request.UserId);
        if (!isMember && circle.CreatedByUserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this circle.");
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
            CreatedByUserName = circle.CreatedByUser?.FullName ?? string.Empty,
            CreatedAt = circle.CreatedAt,
            StartedAt = circle.StartedAt,
            CompletedAt = circle.CompletedAt,
            MemberCount = circle.Members.Count,
            TotalRounds = circle.Rounds.Count,
            CurrentRoundNumber = currentRound?.RoundNumber ?? 0,
            Members = circle.Members
                .OrderBy(m => m.MemberOrder)
                .Select(m => new CircleMemberDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    FullName = m.User?.FullName ?? string.Empty,
                    Email = m.User?.Email ?? string.Empty,
                    MemberOrder = m.MemberOrder,
                    RoleInCircle = m.RoleInCircle,
                    HasReceived = m.HasReceived,
                    JoinedAt = m.JoinedAt
                }).ToList()
        };
    }
}
