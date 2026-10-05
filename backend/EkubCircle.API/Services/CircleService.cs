using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Circles;
using EkubCircle.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Services;

public class CircleService : ICircleService
{
    private readonly EkubDbContext _context;

    public CircleService(EkubDbContext context)
    {
        _context = context;
    }

    public async Task<CircleDetailDto> CreateCircleAsync(int userId, CreateCircleRequestDto request)
    {
        if (request.ContributionAmount <= 0)
        {
            throw new ArgumentException("Contribution amount must be greater than zero.");
        }

        var user = await _context.Users.FindAsync(userId) 
                   ?? throw new KeyNotFoundException("Creator user not found.");

        var circle = new Circle
        {
            Name = request.Name.Trim(),
            ContributionAmount = request.ContributionAmount,
            MeetingLabel = string.IsNullOrWhiteSpace(request.MeetingLabel) ? "Weekly" : request.MeetingLabel.Trim(),
            Status = CircleStatus.Forming,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Circles.Add(circle);
        await _context.SaveChangesAsync();

        // Creator automatically joins as Organizer
        var organizerMember = new CircleMember
        {
            CircleId = circle.Id,
            UserId = userId,
            MemberOrder = 0,
            RoleInCircle = CircleRole.Organizer,
            HasReceived = false,
            JoinedAt = DateTime.UtcNow
        };

        _context.CircleMembers.Add(organizerMember);
        await _context.SaveChangesAsync();

        return await GetCircleDetailsAsync(circle.Id, userId);
    }

    public async Task<List<CircleDto>> GetUserCirclesAsync(int userId, string? status = null)
    {
        var query = _context.Circles
            .Include(c => c.CreatedByUser)
            .Include(c => c.Members)
            .Where(c => c.Members.Any(m => m.UserId == userId));

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status.ToLower() == status.Trim().ToLower());
        }

        var circles = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

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

    public async Task<CircleDetailDto> GetCircleDetailsAsync(int circleId, int userId)
    {
        var circle = await _context.Circles
            .Include(c => c.CreatedByUser)
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .Include(c => c.Rounds)
            .FirstOrDefaultAsync(c => c.Id == circleId)
            ?? throw new KeyNotFoundException("Circle not found.");

        // Check user has access
        if (!circle.Members.Any(m => m.UserId == userId))
        {
            throw new UnauthorizedAccessException("You are not a member of this circle.");
        }

        var currentRound = circle.Rounds.FirstOrDefault(r => r.Status == RoundStatus.Open);

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
            TotalRounds = circle.Rounds.Count,
            CurrentRoundNumber = currentRound?.RoundNumber ?? (circle.Status == CircleStatus.Completed ? circle.Rounds.Count : 0),
            Members = circle.Members
                .OrderBy(m => m.MemberOrder > 0 ? m.MemberOrder : int.MaxValue)
                .ThenBy(m => m.JoinedAt)
                .Select(m => new CircleMemberDto
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

    public async Task<CircleMemberDto> AddMemberAsync(int circleId, int organizerUserId, AddMemberRequestDto request)
    {
        var circle = await _context.Circles
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == circleId)
            ?? throw new KeyNotFoundException("Circle not found.");

        // Verify organizer permissions
        var caller = circle.Members.FirstOrDefault(m => m.UserId == organizerUserId);
        if (caller == null || caller.RoleInCircle != CircleRole.Organizer)
        {
            throw new UnauthorizedAccessException("Only the circle organizer can add members.");
        }

        // Hard rule: Member list is locked after circle starts
        if (circle.Status != CircleStatus.Forming)
        {
            throw new InvalidOperationException("Cannot add members to an active or completed circle. Member roster is locked.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail)
            ?? throw new KeyNotFoundException($"No registered user found with email '{request.Email}'.");

        if (circle.Members.Any(m => m.UserId == targetUser.Id))
        {
            throw new InvalidOperationException("User is already a member of this circle.");
        }

        var member = new CircleMember
        {
            CircleId = circle.Id,
            UserId = targetUser.Id,
            MemberOrder = 0,
            RoleInCircle = CircleRole.Member,
            HasReceived = false,
            JoinedAt = DateTime.UtcNow
        };

        _context.CircleMembers.Add(member);
        await _context.SaveChangesAsync();

        return new CircleMemberDto
        {
            Id = member.Id,
            UserId = targetUser.Id,
            FullName = targetUser.FullName,
            Email = targetUser.Email,
            MemberOrder = member.MemberOrder,
            RoleInCircle = member.RoleInCircle,
            HasReceived = member.HasReceived,
            JoinedAt = member.JoinedAt
        };
    }

    public async Task RemoveMemberAsync(int circleId, int organizerUserId, int memberId)
    {
        var circle = await _context.Circles
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == circleId)
            ?? throw new KeyNotFoundException("Circle not found.");

        var caller = circle.Members.FirstOrDefault(m => m.UserId == organizerUserId);
        if (caller == null || caller.RoleInCircle != CircleRole.Organizer)
        {
            throw new UnauthorizedAccessException("Only the organizer can remove members.");
        }

        if (circle.Status != CircleStatus.Forming)
        {
            throw new InvalidOperationException("Cannot remove members once the circle has started.");
        }

        var targetMember = circle.Members.FirstOrDefault(m => m.Id == memberId)
            ?? throw new KeyNotFoundException("Member not found in this circle.");

        if (targetMember.UserId == organizerUserId)
        {
            throw new InvalidOperationException("Organizer cannot remove themselves from their own circle.");
        }

        _context.CircleMembers.Remove(targetMember);
        await _context.SaveChangesAsync();
    }

    public async Task<CircleDetailDto> StartCircleAsync(int circleId, int organizerUserId)
    {
        var circle = await _context.Circles
            .Include(c => c.Members)
            .Include(c => c.Rounds)
            .FirstOrDefaultAsync(c => c.Id == circleId)
            ?? throw new KeyNotFoundException("Circle not found.");

        var caller = circle.Members.FirstOrDefault(m => m.UserId == organizerUserId);
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

        // 1. Assign deterministic MemberOrder (1 to N)
        // Organizer is #1 or sequential assignment based on JoinedAt order
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

        await _context.SaveChangesAsync();

        return await GetCircleDetailsAsync(circle.Id, organizerUserId);
    }
}
