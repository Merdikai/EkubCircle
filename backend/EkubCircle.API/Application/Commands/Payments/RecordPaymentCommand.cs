using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Payments;
using EkubCircle.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Commands.Payments;

public record RecordPaymentCommand(int CallerUserId, int RoundId, int MemberId, decimal Amount, string PaymentMethod, string? Notes) : IRequest<PaymentDto>;

public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, PaymentDto>
{
    private readonly EkubDbContext _context;

    public RecordPaymentCommandHandler(EkubDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var round = await _context.Rounds
            .Include(r => r.Circle)
                .ThenInclude(c => c!.Members)
                    .ThenInclude(m => m.User)
            .Include(r => r.Payments)
            .FirstOrDefaultAsync(r => r.Id == request.RoundId, cancellationToken)
            ?? throw new KeyNotFoundException("Round not found.");

        var circle = round.Circle!;

        // 1. Authorization: Only Organizer or Admin can record payments
        var callerMember = circle.Members.FirstOrDefault(m => m.UserId == request.CallerUserId);
        var callerUser = await _context.Users.FindAsync(new object[] { request.CallerUserId }, cancellationToken);
        bool isAdmin = callerUser != null && callerUser.Role == UserRole.Admin;

        if (!isAdmin && (callerMember == null || callerMember.RoleInCircle != CircleRole.Organizer))
        {
            throw new UnauthorizedAccessException("Only the circle organizer or an administrator can record payments.");
        }

        // 2. Circle status check
        if (circle.Status != CircleStatus.Active)
        {
            throw new InvalidOperationException($"Cannot record payments: Circle is currently '{circle.Status}'. Payments can only be recorded for 'Active' circles.");
        }

        // 3. Round status check
        if (round.Status != RoundStatus.Open)
        {
            throw new InvalidOperationException($"Cannot record payment: Round {round.RoundNumber} is '{round.Status}', not 'Open'.");
        }

        // 4. Verify target member belongs to this circle
        var targetMember = circle.Members.FirstOrDefault(m => m.Id == request.MemberId)
            ?? throw new KeyNotFoundException("Member does not belong to this circle.");

        // 5. Amount check: Must match circle's defined contribution amount
        if (request.Amount != circle.ContributionAmount)
        {
            throw new ArgumentException($"Payment amount must exactly match the circle's contribution amount of {circle.ContributionAmount:N2} ETB.");
        }

        // 6. Hard Duplicate Check: Has member already paid for this round?
        bool alreadyPaid = round.Payments.Any(p => p.MemberId == request.MemberId && p.PaymentType == PaymentType.Contribution);
        if (alreadyPaid)
        {
            throw new InvalidOperationException($"Member '{targetMember.User?.FullName}' has already contributed for Round {round.RoundNumber}. Duplicate contributions for the same round are forbidden.");
        }

        // 7. Create Payment ledger entry
        var payment = new Payment
        {
            RoundId = round.Id,
            MemberId = targetMember.Id,
            Amount = request.Amount,
            PaymentType = PaymentType.Contribution,
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod.Trim(),
            Notes = request.Notes?.Trim(),
            PaidAt = DateTime.UtcNow,
            RecordedByUserId = request.CallerUserId
        };

        _context.Payments.Add(payment);

        // Update Round pot amount dynamically
        var distinctPaidCount = round.Payments
            .Where(p => p.PaymentType == PaymentType.Contribution)
            .Select(p => p.MemberId)
            .Distinct()
            .Count() + 1; // + 1 for current payment

        round.PotAmount = distinctPaidCount * circle.ContributionAmount;

        await _context.SaveChangesAsync(cancellationToken);

        return new PaymentDto
        {
            Id = payment.Id,
            RoundId = round.Id,
            RoundNumber = round.RoundNumber,
            MemberId = targetMember.Id,
            MemberName = targetMember.User?.FullName ?? "Unknown",
            Amount = payment.Amount,
            PaymentType = payment.PaymentType,
            PaymentMethod = payment.PaymentMethod,
            Notes = payment.Notes,
            PaidAt = payment.PaidAt
        };
    }
}
