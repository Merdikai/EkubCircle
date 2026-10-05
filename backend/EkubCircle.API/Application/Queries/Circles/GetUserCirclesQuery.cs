using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Circles;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Queries.Circles;

public record GetUserCirclesQuery(int UserId, string? Status = null) : IRequest<List<CircleDto>>;

public class GetUserCirclesQueryHandler : IRequestHandler<GetUserCirclesQuery, List<CircleDto>>
{
    private readonly EkubDbContext _context;

    public GetUserCirclesQueryHandler(EkubDbContext context)
    {
        _context = context;
    }

    public async Task<List<CircleDto>> Handle(GetUserCirclesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Circles
            .Include(c => c.CreatedByUser)
            .Include(c => c.Members)
            .Where(c => c.Members.Any(m => m.UserId == request.UserId));

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(c => c.Status.ToLower() == request.Status.Trim().ToLower());
        }

        var circles = await query.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);

        return circles.Select(c => new CircleDto
        {
            Id = c.Id,
            Name = c.Name,
            ContributionAmount = c.ContributionAmount,
            MeetingLabel = c.MeetingLabel,
            Status = c.Status,
            CreatedByUserId = c.CreatedByUserId,
            CreatedByUserName = c.CreatedByUser?.FullName ?? "Unknown",
            CreatedAt = c.CreatedAt,
            StartedAt = c.StartedAt,
            CompletedAt = c.CompletedAt,
            MemberCount = c.Members.Count
        }).ToList();
    }
}
