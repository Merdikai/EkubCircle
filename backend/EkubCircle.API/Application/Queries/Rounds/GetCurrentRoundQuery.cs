using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Rounds;
using EkubCircle.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Queries.Rounds;

public record GetCurrentRoundQuery(int CircleId, int UserId) : IRequest<CurrentRoundDto>;

public class GetCurrentRoundQueryHandler : IRequestHandler<GetCurrentRoundQuery, CurrentRoundDto>
{
    private readonly EkubDbContext _context;

    public GetCurrentRoundQueryHandler(EkubDbContext context)
    {
        _context = context;
    }

    public async Task<CurrentRoundDto> Handle(GetCurrentRoundQuery request, CancellationToken cancellationToken)
    {
        var circle = await _context.Circles
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .Include(c => c.Rounds)
                .ThenInclude(r => r.Payments)
            .Include(c => c.Rounds)
                .ThenInclude(r => r.ReceiverMember)
                    .ThenInclude(rm => rm!.User)
            .FirstOrDefaultAsync(c => c.Id == request.CircleId, cancellationToken)
            ?? throw new KeyNotFoundException("Circle not found.");

        if (!circle.Members.Any(m => m.UserId == request.UserId))
        {
            throw new UnauthorizedAccessException("You are not a member of this circle.");
        }

        var round = circle.Rounds.FirstOrDefault(r => r.Status == RoundStatus.Open)
                    ?? circle.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault()
                    ?? throw new InvalidOperationException("No rounds found for this circle. The circle may not have started yet.");

        var receiver = round.ReceiverMember;
        var contributionPayments = round.Payments.Where(p => p.PaymentType == PaymentType.Contribution).ToList();

        var memberDtos = circle.Members
            .OrderBy(m => m.MemberOrder > 0 ? m.MemberOrder : int.MaxValue)
            .Select(m =>
            {
                var payment = contributionPayments.FirstOrDefault(p => p.MemberId == m.Id);
                return new CurrentRoundMemberDto
                {
                    MemberId = m.Id,
                    UserId = m.UserId,
                    FullName = m.User?.FullName ?? "Unknown",
                    Email = m.User?.Email ?? "Unknown",
                    MemberOrder = m.MemberOrder,
                    HasReceived = m.HasReceived,
                    HasPaidThisRound = payment != null,
                    AmountPaid = payment?.Amount,
                    PaidAt = payment?.PaidAt,
                    PaymentMethod = payment?.PaymentMethod,
                    Notes = payment?.Notes
                };
            }).ToList();

        int totalMembers = circle.Members.Count;
        int paidCount = contributionPayments.Select(p => p.MemberId).Distinct().Count();
        decimal currentPot = paidCount * circle.ContributionAmount;
        decimal targetPot = totalMembers * circle.ContributionAmount;

        return new CurrentRoundDto
        {
            RoundId = round.Id,
            CircleId = circle.Id,
            CircleName = circle.Name,
            RoundNumber = round.RoundNumber,
            TotalRounds = circle.Rounds.Count,
            Status = round.Status,
            ReceiverMemberId = receiver?.Id ?? 0,
            ReceiverFullName = receiver?.User?.FullName ?? "Unknown",
            ReceiverEmail = receiver?.User?.Email ?? "Unknown",
            ReceiverMemberOrder = receiver?.MemberOrder ?? 0,
            ContributionAmount = circle.ContributionAmount,
            TargetPotAmount = targetPot,
            CurrentPotAmount = currentPot,
            TotalMembers = totalMembers,
            PaidCount = paidCount,
            IsReadyForPayout = round.Status == RoundStatus.Open && paidCount == totalMembers,
            Members = memberDtos
        };
    }
}
