using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Rounds;
using EkubCircle.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Queries.Rounds;

public record GetCircleRoundsQuery(int CircleId, int UserId) : IRequest<List<RoundSummaryDto>>;

public class GetCircleRoundsQueryHandler : IRequestHandler<GetCircleRoundsQuery, List<RoundSummaryDto>>
{
    private readonly EkubDbContext _context;

    public GetCircleRoundsQueryHandler(EkubDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoundSummaryDto>> Handle(GetCircleRoundsQuery request, CancellationToken cancellationToken)
    {
        var circle = await _context.Circles
            .Include(c => c.Members)
            .Include(c => c.Rounds)
                .ThenInclude(r => r.ReceiverMember)
                    .ThenInclude(rm => rm!.User)
            .Include(c => c.Rounds)
                .ThenInclude(r => r.Payments)
            .FirstOrDefaultAsync(c => c.Id == request.CircleId, cancellationToken)
            ?? throw new KeyNotFoundException("Circle not found.");

        if (!circle.Members.Any(m => m.UserId == request.UserId))
        {
            throw new UnauthorizedAccessException("You are not a member of this circle.");
        }

        int totalMembers = circle.Members.Count;

        return circle.Rounds
            .OrderBy(r => r.RoundNumber)
            .Select(r => new RoundSummaryDto
            {
                RoundId = r.Id,
                RoundNumber = r.RoundNumber,
                Status = r.Status,
                ReceiverMemberId = r.ReceiverMemberId,
                ReceiverName = r.ReceiverMember?.User?.FullName ?? "Unknown",
                PotAmount = r.Status == RoundStatus.PaidOut ? r.PotAmount : (r.Payments.Count(p => p.PaymentType == PaymentType.Contribution) * circle.ContributionAmount),
                PaidOutAt = r.PaidOutAt,
                PaidCount = r.Payments.Count(p => p.PaymentType == PaymentType.Contribution),
                TotalMembers = totalMembers
            }).ToList();
    }
}
