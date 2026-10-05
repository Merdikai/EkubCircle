using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.DTOs.Rounds;
using EkubCircle.Application.Queries.Rounds;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Application.Handlers.Rounds;

public class GetCurrentRoundQueryHandler : IRequestHandler<GetCurrentRoundQuery, CurrentRoundDto>
{
    private readonly IEkubDbContext _context;

    public GetCurrentRoundQueryHandler(IEkubDbContext context)
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

        var activeRound = circle.Rounds.FirstOrDefault(r => r.Status == RoundStatus.Open);
        if (activeRound == null)
        {
            // If none open, check for the latest round
            activeRound = circle.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault();
        }

        if (activeRound == null)
        {
            throw new InvalidOperationException("No rounds exist for this circle. The circle has not been started.");
        }

        var normalPayments = activeRound.Payments.Where(p => p.PaymentType == PaymentType.Contribution).ToList();
        var totalMembers = circle.Members.Count;
        var paidCount = normalPayments.Count;
        var isReady = paidCount >= totalMembers && activeRound.Status == RoundStatus.Open;
        var currentPot = normalPayments.Sum(p => p.Amount);
        var targetPot = totalMembers * circle.ContributionAmount;

        var receiver = activeRound.ReceiverMember;

        var memberDtos = circle.Members
            .OrderBy(m => m.MemberOrder)
            .Select(m =>
            {
                var payment = normalPayments.FirstOrDefault(p => p.MemberId == m.Id);
                return new CurrentRoundMemberDto
                {
                    MemberId = m.Id,
                    UserId = m.UserId,
                    FullName = m.User?.FullName ?? string.Empty,
                    Email = m.User?.Email ?? string.Empty,
                    MemberOrder = m.MemberOrder,
                    HasReceived = m.HasReceived,
                    HasPaidThisRound = payment != null,
                    AmountPaid = payment?.Amount,
                    PaidAt = payment?.PaidAt,
                    PaymentMethod = payment?.PaymentMethod,
                    Notes = payment?.Notes
                };
            }).ToList();

        return new CurrentRoundDto
        {
            RoundId = activeRound.Id,
            CircleId = circle.Id,
            CircleName = circle.Name,
            RoundNumber = activeRound.RoundNumber,
            TotalRounds = circle.Rounds.Count,
            Status = activeRound.Status,
            ReceiverMemberId = receiver?.Id ?? 0,
            ReceiverFullName = receiver?.User?.FullName ?? string.Empty,
            ReceiverEmail = receiver?.User?.Email ?? string.Empty,
            ReceiverMemberOrder = receiver?.MemberOrder ?? 0,
            ContributionAmount = circle.ContributionAmount,
            TargetPotAmount = targetPot,
            CurrentPotAmount = currentPot,
            TotalMembers = totalMembers,
            PaidCount = paidCount,
            IsReadyForPayout = isReady,
            Members = memberDtos
        };
    }
}
