using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Commands.Circles;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Application.Handlers.Circles;

public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Unit>
{
    private readonly IEkubDbContext _context;

    public RemoveMemberCommandHandler(IEkubDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var circle = await _context.Circles
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == request.CircleId, cancellationToken);

        if (circle == null)
        {
            throw new KeyNotFoundException($"Circle with ID {request.CircleId} was not found.");
        }

        if (circle.CreatedByUserId != request.RequesterUserId)
        {
            throw new UnauthorizedAccessException("Only the circle organizer can remove members.");
        }

        if (circle.Status != CircleStatus.Forming)
        {
            throw new InvalidOperationException($"Cannot remove members from circle with status '{circle.Status}'.");
        }

        var member = circle.Members.FirstOrDefault(m => m.Id == request.MemberId);
        if (member == null)
        {
            throw new KeyNotFoundException($"Member with ID {request.MemberId} was not found in this circle.");
        }

        if (member.UserId == circle.CreatedByUserId)
        {
            throw new InvalidOperationException("The circle organizer cannot be removed from the circle.");
        }

        _context.CircleMembers.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);

        // Re-order remaining members
        var remainingMembers = circle.Members
            .Where(m => m.Id != member.Id)
            .OrderBy(m => m.MemberOrder)
            .ToList();

        for (int i = 0; i < remainingMembers.Count; i++)
        {
            remainingMembers[i].MemberOrder = i + 1;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
