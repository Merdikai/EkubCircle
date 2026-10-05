using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.DTOs.Rounds;
using EkubCircle.Application.Queries.Rounds;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Application.Handlers.Rounds;

public class GetCircleRoundsQueryHandler : IRequestHandler<GetCircleRoundsQuery, List<RoundSummaryDto>>
{
    private readonly IEkubDbContext _context;

    public GetCircleRoundsQueryHandler(IEkubDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoundSummaryDto>> Handle(GetCircleRoundsQuery request, CancellationToken cancellationToken)
    {
        var circle = await _context.Circles
            .Include(c => c.Members)
            .Include(c => c.Rounds)
                .ThenInclude(r => r.Payments)
            .Include(c => c.Rounds)
                .ThenInclude(r => r.WinnerMember)
                    .ThenInclude(rm => rm!.User)
            .FirstOrDefaultAsync(c => c.Id == request.CircleId, cancellationToken);

        if (circle == null)
        {
            throw new KeyNotFoundException($"Circle with ID {request.CircleId} was not found.");
        }

        var isMember = circle.Members.Any(m => m.UserId == request.UserId);
        if (!isMember && circle.CreatedByUserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to view rounds for this circle.");
        }

        var totalMembers = circle.Members.Count;

        return circle.Rounds
            .OrderBy(r => r.RoundNumber)
            .Select(r => new RoundSummaryDto
            {
                RoundId = r.Id,
                RoundNumber = r.RoundNumber,
                Status = r.Status,
                ReceiverMemberId = r.WinnerMemberId ?? 0,
                ReceiverName = r.WinnerMember != null && r.WinnerMember.User != null ? r.WinnerMember.User.FullName : string.Empty,
                PotAmount = r.PotAmount,
                PaidOutAt = r.PaidOutAt,
                PaidCount = r.Payments.Count(p => p.PaymentType == PaymentType.Contribution),
                TotalMembers = totalMembers
            }).ToList();
    }
}
