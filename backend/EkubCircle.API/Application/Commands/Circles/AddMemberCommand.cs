using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Circles;
using EkubCircle.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Commands.Circles;

public record AddMemberCommand(int CircleId, int OrganizerUserId, string Email) : IRequest<CircleMemberDto>;

public class AddMemberCommandHandler : IRequestHandler<AddMemberCommand, CircleMemberDto>
{
    private readonly EkubDbContext _context;

    public AddMemberCommandHandler(EkubDbContext context)
    {
        _context = context;
    }

    public async Task<CircleMemberDto> Handle(AddMemberCommand request, CancellationToken cancellationToken)
    {
        var circle = await _context.Circles
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == request.CircleId, cancellationToken)
            ?? throw new KeyNotFoundException("Circle not found.");

        var caller = circle.Members.FirstOrDefault(m => m.UserId == request.OrganizerUserId);
        if (caller == null || caller.RoleInCircle != CircleRole.Organizer)
        {
            throw new UnauthorizedAccessException("Only the circle organizer can add members.");
        }

        if (circle.Status != CircleStatus.Forming)
        {
            throw new InvalidOperationException("Cannot add members to an active or completed circle. Member roster is locked.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken)
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
        await _context.SaveChangesAsync(cancellationToken);

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
}
