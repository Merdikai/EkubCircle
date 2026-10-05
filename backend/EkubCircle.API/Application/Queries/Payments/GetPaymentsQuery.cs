using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Queries.Payments;

public record GetPaymentsQuery(int? CircleId, int? RoundId, int CallerUserId) : IRequest<List<PaymentDto>>;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, List<PaymentDto>>
{
    private readonly EkubDbContext _context;

    public GetPaymentsQueryHandler(EkubDbContext context)
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

        if (request.RoundId.HasValue)
        {
            query = query.Where(p => p.RoundId == request.RoundId.Value);
        }

        if (request.CircleId.HasValue)
        {
            query = query.Where(p => p.Round!.CircleId == request.CircleId.Value);
        }

        var payments = await query.OrderByDescending(p => p.PaidAt).ToListAsync(cancellationToken);

        return payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            RoundId = p.RoundId,
            RoundNumber = p.Round?.RoundNumber ?? 0,
            MemberId = p.MemberId,
            MemberName = p.Member?.User?.FullName ?? "Unknown",
            Amount = p.Amount,
            PaymentType = p.PaymentType,
            PaymentMethod = p.PaymentMethod,
            Notes = p.Notes,
            PaidAt = p.PaidAt
        }).ToList();
    }
}
