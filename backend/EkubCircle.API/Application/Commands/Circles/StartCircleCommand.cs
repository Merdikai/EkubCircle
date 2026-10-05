using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Circles;
using EkubCircle.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Commands.Circles;

public record StartCircleCommand(int CircleId, int OrganizerUserId) : IRequest<CircleDetailDto>;

public class StartCircleCommandHandler : IRequestHandler<StartCircleCommand, CircleDetailDto>
{
    private readonly EkubDbContext _context;

    public StartCircleCommandHandler(EkubDbContext context)
    {
        _context = context;
    }

    public async Task<CircleDetailDto> Handle(StartCircleCommand request, CancellationToken cancellationToken)
    {
        var circle = await _context.Circles
            .Include(c => c.CreatedByUser)
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .Include(c => c.Rounds)
            .FirstOrDefaultAsync(c => c.Id == request.CircleId, cancellationToken)
            ?? throw new KeyNotFoundException("Circle not found.");

        var caller = circle.Members.FirstOrDefault(m => m.UserId == request.OrganizerUserId);
        if (caller == null || caller.RoleInCircle != CircleRole.Organizer)
        {
            throw new UnauthorizedAccessException("Only the organizer can start the circle.");
        }

        if (circle.Status != CircleStatus.Forming)
        {
            throw new InvalidOperationException($"Circle cannot be started because its current status is '{circle.Status}'.");
        }

        if (circle.Members.Count < 2)
        {
            throw new InvalidOperationException("An Ekub circle requires at least 2 members to start.");
        }

        // 1. Assign deterministic MemberOrder (1 to N) based on JoinedAt
        var sortedMembers = circle.Members.OrderBy(m => m.JoinedAt).ToList();
        for (int i = 0; i < sortedMembers.Count; i++)
        {
            sortedMembers[i].MemberOrder = i + 1;
            sortedMembers[i].HasReceived = false;
        }

        // 2. Transition Circle status to Active
        circle.Status = CircleStatus.Active;
        circle.StartedAt = DateTime.UtcNow;

        // 3. Pre-create N rounds with pre-assigned receivers
        int totalMembers = sortedMembers.Count;
        for (int k = 1; k <= totalMembers; k++)
        {
            var receiver = sortedMembers.First(m => m.MemberOrder == k);
            var round = new Round
            {
                CircleId = circle.Id,
                RoundNumber = k,
                ReceiverMemberId = receiver.Id,
                Status = k == 1 ? RoundStatus.Open : "Pending",
                PotAmount = 0m,
                PaidOutAt = null
            };
            _context.Rounds.Add(round);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CircleDetailDto
        {
            Id = circle.Id,
            Name = circle.Name,
            ContributionAmount = circle.ContributionAmount,
            MeetingLabel = circle.MeetingLabel,
            Status = circle.Status,
            CreatedByUserId = circle.CreatedByUserId,
            CreatedByUserName = circle.CreatedByUser?.FullName ?? "Unknown",
            CreatedAt = circle.CreatedAt,
            StartedAt = circle.StartedAt,
            CompletedAt = circle.CompletedAt,
            MemberCount = circle.Members.Count,
            TotalRounds = totalMembers,
            CurrentRoundNumber = 1,
            Members = sortedMembers.Select(m => new CircleMemberDto
            {
                Id = m.Id,
                UserId = m.UserId,
                FullName = m.User?.FullName ?? "Unknown",
                Email = m.User?.Email ?? "Unknown",
                MemberOrder = m.MemberOrder,
                RoleInCircle = m.RoleInCircle,
                HasReceived = m.HasReceived,
                JoinedAt = m.JoinedAt
            }).ToList()
        };
    }
}
