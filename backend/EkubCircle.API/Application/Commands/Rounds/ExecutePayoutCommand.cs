using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Rounds;
using EkubCircle.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Commands.Rounds;

public record ExecutePayoutCommand(int RoundId, int CallerUserId) : IRequest<PayoutResultDto>;

public class ExecutePayoutCommandHandler : IRequestHandler<ExecutePayoutCommand, PayoutResultDto>
{
    private readonly EkubDbContext _context;

    public ExecutePayoutCommandHandler(EkubDbContext context)
    {
        _context = context;
    }

    public async Task<PayoutResultDto> Handle(ExecutePayoutCommand request, CancellationToken cancellationToken)
    {
        var round = await _context.Rounds
            .Include(r => r.Circle)
                .ThenInclude(c => c!.Members)
                    .ThenInclude(m => m.User)
            .Include(r => r.ReceiverMember)
                .ThenInclude(rm => rm!.User)
            .Include(r => r.Payments)
            .FirstOrDefaultAsync(r => r.Id == request.RoundId, cancellationToken)
            ?? throw new KeyNotFoundException("Round not found.");

        var circle = round.Circle!;

        // 1. Verify caller is Organizer or Admin
        var callerMember = circle.Members.FirstOrDefault(m => m.UserId == request.CallerUserId);
        var callerUser = await _context.Users.FindAsync(new object[] { request.CallerUserId }, cancellationToken);
        bool isAdmin = callerUser != null && callerUser.Role == UserRole.Admin;

        if (!isAdmin && (callerMember == null || callerMember.RoleInCircle != CircleRole.Organizer))
        {
            throw new UnauthorizedAccessException("Only the circle organizer or an administrator can execute the pot payout.");
        }

        // 2. Verify Round is currently Open
        if (round.Status != RoundStatus.Open)
        {
            throw new InvalidOperationException($"Cannot execute payout: Round status is '{round.Status}', but must be 'Open'.");
        }

        // 3. Verify Single Pot Receipt rule: Has this member already received a payout?
        var receiver = round.ReceiverMember!;
        if (receiver.HasReceived)
        {
            throw new InvalidOperationException($"Member '{receiver.User?.FullName}' has already received a payout in this circle. Ekub rules strictly forbid duplicate payouts.");
        }

        // 4. Verify 100% Contribution Gate
        int totalMembers = circle.Members.Count;
        var paidMemberIds = round.Payments
            .Where(p => p.PaymentType == PaymentType.Contribution)
            .Select(p => p.MemberId)
            .Distinct()
            .ToHashSet();

        if (paidMemberIds.Count < totalMembers)
        {
            int missingCount = totalMembers - paidMemberIds.Count;
            throw new InvalidOperationException($"Cannot payout pot: Not all members have contributed for round {round.RoundNumber}. " +
                                                $"{paidMemberIds.Count} of {totalMembers} members paid. {missingCount} contribution(s) missing.");
        }

        // 5. Payout Execution
        decimal potAmount = totalMembers * circle.ContributionAmount;
        var now = DateTime.UtcNow;

        receiver.HasReceived = true;
        round.PotAmount = potAmount;
        round.Status = RoundStatus.PaidOut;
        round.PaidOutAt = now;

        var payoutPayment = new Payment
        {
            RoundId = round.Id,
            MemberId = receiver.Id,
            Amount = potAmount,
            PaymentType = PaymentType.Payout,
            PaymentMethod = "Ledger Pot Payout",
            Notes = $"Payout for Round {round.RoundNumber} to {receiver.User?.FullName}",
            PaidAt = now,
            RecordedByUserId = request.CallerUserId
        };
        _context.Payments.Add(payoutPayment);

        // 6. Transition to Next Round or Complete Circle
        var allRounds = await _context.Rounds
            .Where(r => r.CircleId == circle.Id)
            .OrderBy(r => r.RoundNumber)
            .ToListAsync(cancellationToken);

        var nextRound = allRounds.FirstOrDefault(r => r.RoundNumber == round.RoundNumber + 1);
        int? nextRoundNumber = null;

        if (nextRound != null)
        {
            nextRound.Status = RoundStatus.Open;
            nextRoundNumber = nextRound.RoundNumber;
        }
        else
        {
            circle.Status = CircleStatus.Completed;
            circle.CompletedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new PayoutResultDto
        {
            RoundId = round.Id,
            RoundNumber = round.RoundNumber,
            PotAmount = potAmount,
            ReceiverMemberId = receiver.Id,
            ReceiverName = receiver.User?.FullName ?? "Unknown",
            PaidOutAt = now,
            RoundStatus = round.Status,
            CircleStatus = circle.Status,
            NextRoundNumber = nextRoundNumber,
            Message = nextRoundNumber.HasValue 
                ? $"Pot of {potAmount:N2} ETB successfully paid out to {receiver.User?.FullName}. Round {nextRoundNumber} is now Open."
                : $"Pot of {potAmount:N2} ETB successfully paid out to {receiver.User?.FullName}. All rounds are finished; Circle is now Completed!"
        };
    }
}
