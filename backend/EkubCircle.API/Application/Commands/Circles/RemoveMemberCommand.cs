using EkubCircle.API.Data;
using EkubCircle.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Commands.Circles;

public record RemoveMemberCommand(int CircleId, int OrganizerUserId, int MemberId) : IRequest;

public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand>
{
    private readonly EkubDbContext _context;

    public RemoveMemberCommandHandler(EkubDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var circle = await _context.Circles
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == request.CircleId, cancellationToken)
            ?? throw new KeyNotFoundException("Circle not found.");

        var caller = circle.Members.FirstOrDefault(m => m.UserId == request.OrganizerUserId);
        if (caller == null || caller.RoleInCircle != CircleRole.Organizer)
        {
            throw new UnauthorizedAccessException("Only the organizer can remove members.");
        }

        if (circle.Status != CircleStatus.Forming)
        {
            throw new InvalidOperationException("Cannot remove members once the circle has started.");
        }

        var targetMember = circle.Members.FirstOrDefault(m => m.Id == request.MemberId)
            ?? throw new KeyNotFoundException("Member not found in this circle.");

        if (targetMember.UserId == request.OrganizerUserId)
        {
            throw new InvalidOperationException("Organizer cannot remove themselves from their own circle.");
        }

        _context.CircleMembers.Remove(targetMember);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
