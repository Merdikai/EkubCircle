using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.DTOs.Circles;
using EkubCircle.Application.Queries.Circles;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Application.Handlers.Circles;

public class GetCircleSummaryQueryHandler : IRequestHandler<GetCircleSummaryQuery, CircleSummaryDto>
{
    private readonly IEkubDbContext _context;

    public GetCircleSummaryQueryHandler(IEkubDbContext context)
    {
        _context = context;
    }

    public async Task<CircleSummaryDto> Handle(GetCircleSummaryQuery request, CancellationToken cancellationToken)
    {
        var circle = await _context.Circles
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .Include(c => c.Rounds)
                .ThenInclude(r => r.WinnerMember)
                    .ThenInclude(rm => rm!.User)
            .Include(c => c.Rounds)
                .ThenInclude(r => r.Payments)
            .FirstOrDefaultAsync(c => c.Id == request.CircleId, cancellationToken);

        if (circle == null)
        {
            throw new KeyNotFoundException($"Circle with ID {request.CircleId} was not found.");
        }

        var isMember = circle.Members.Any(m => m.UserId == request.UserId);
        if (!isMember && circle.CreatedByUserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this circle's summary.");
        }

        var paidOutRounds = circle.Rounds.Where(r => r.Status == RoundStatus.PaidOut).ToList();
        var totalDisbursed = paidOutRounds.Sum(r => r.PotAmount);

        var roundSummaries = circle.Rounds
            .OrderBy(r => r.RoundNumber)
            .Select(r => new CircleSummaryRoundDto
            {
                RoundNumber = r.RoundNumber,
                Status = r.Status,
                ReceiverName = r.WinnerMember?.User?.FullName ?? string.Empty,
                PotAmount = r.PotAmount,
                PaidOutAt = r.PaidOutAt
            }).ToList();

        var memberSummaries = circle.Members
            .OrderBy(m => m.MemberOrder)
            .Select(m => new CircleSummaryMemberDto
            {
                MemberId = m.Id,
                FullName = m.User?.FullName ?? string.Empty,
                Email = m.User?.Email ?? string.Empty,
                MemberOrder = m.MemberOrder,
                HasReceived = m.HasReceived,
                TotalContributionsPaid = circle.Rounds
                    .SelectMany(r => r.Payments)
                    .Count(p => p.MemberId == m.Id && p.PaymentType == PaymentType.Contribution)
            }).ToList();

        return new CircleSummaryDto
        {
            CircleId = circle.Id,
            Name = circle.Name,
            Status = circle.Status,
            ContributionAmount = circle.ContributionAmount,
            MeetingLabel = circle.MeetingLabel,
            TotalMembers = circle.Members.Count,
            TotalRounds = circle.Rounds.Count,
            CompletedRoundsCount = paidOutRounds.Count,
            TotalPotDisbursed = totalDisbursed,
            CreatedAt = circle.CreatedAt,
            StartedAt = circle.StartedAt,
            CompletedAt = circle.CompletedAt,
            Rounds = roundSummaries,
            Members = memberSummaries
        };
    }
}
