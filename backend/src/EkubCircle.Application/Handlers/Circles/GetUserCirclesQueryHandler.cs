using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.DTOs.Circles;
using EkubCircle.Application.Queries.Circles;

namespace EkubCircle.Application.Handlers.Circles;

public class GetUserCirclesQueryHandler : IRequestHandler<GetUserCirclesQuery, List<CircleDto>>
{
    private readonly IEkubDbContext _context;

    public GetUserCirclesQueryHandler(IEkubDbContext context)
    {
        _context = context;
    }

    public async Task<List<CircleDto>> Handle(GetUserCirclesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Circles
            .Include(c => c.CreatedByUser)
            .Include(c => c.Members)
            .Where(c => c.Members.Any(m => m.UserId == request.UserId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(c => c.Status.ToLower() == request.Status.Trim().ToLower());
        }

        var circles = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return circles.Select(c => new CircleDto
        {
            Id = c.Id,
            Name = c.Name,
            ContributionAmount = c.ContributionAmount,
            MeetingLabel = c.MeetingLabel,
            Status = c.Status,
            CreatedByUserId = c.CreatedByUserId,
            CreatedByUserName = c.CreatedByUser?.FullName ?? string.Empty,
            CreatedAt = c.CreatedAt,
            StartedAt = c.StartedAt,
            CompletedAt = c.CompletedAt,
            MemberCount = c.Members.Count
        }).ToList();
    }
}
