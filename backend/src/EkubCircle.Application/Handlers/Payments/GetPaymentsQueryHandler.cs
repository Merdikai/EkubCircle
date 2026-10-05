using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.DTOs.Payments;
using EkubCircle.Application.Queries.Payments;

namespace EkubCircle.Application.Handlers.Payments;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, List<PaymentDto>>
{
    private readonly IEkubDbContext _context;

    public GetPaymentsQueryHandler(IEkubDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .Include(p => p.Round)
            .Include(p => p.Member)
                .ThenInclude(m => m!.User)
            .AsQueryable();

        if (request.RoundId.HasValue && request.RoundId.Value > 0)
        {
            query = query.Where(p => p.RoundId == request.RoundId.Value);
        }
        else if (request.CircleId.HasValue && request.CircleId.Value > 0)
        {
            query = query.Where(p => p.Round != null && p.Round.CircleId == request.CircleId.Value);
        }

        var payments = await query
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync(cancellationToken);

        return payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            RoundId = p.RoundId,
            RoundNumber = p.Round?.RoundNumber ?? 0,
            MemberId = p.MemberId,
            MemberName = p.Member?.User?.FullName ?? string.Empty,
            Amount = p.Amount,
            PaymentType = p.PaymentType,
            PaymentMethod = p.PaymentMethod,
            Notes = p.Notes,
            IsLate = p.IsLate,
            PaidAt = p.PaidAt
        }).ToList();
    }
}
