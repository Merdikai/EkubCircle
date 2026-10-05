using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Commands.Circles;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.DTOs.Circles;
using EkubCircle.Domain.Entities;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Application.Handlers.Circles;

public class AddMemberCommandHandler : IRequestHandler<AddMemberCommand, CircleMemberDto>
{
    private readonly IEkubDbContext _context;

    public AddMemberCommandHandler(IEkubDbContext context)
    {
        _context = context;
    }

    public async Task<CircleMemberDto> Handle(AddMemberCommand request, CancellationToken cancellationToken)
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
            var isOrganizer = circle.Members.Any(m => m.UserId == request.RequesterUserId && m.RoleInCircle == CircleRole.Organizer);
            if (!isOrganizer)
            {
                throw new UnauthorizedAccessException("Only the circle organizer can add members.");
            }
        }

        if (circle.Status != CircleStatus.Forming)
        {
            throw new InvalidOperationException($"Cannot add members to circle with status '{circle.Status}'. Roster is locked once active.");
        }

        var normalizedEmail = request.MemberEmail.Trim().ToLowerInvariant();
        var userToAdd = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (userToAdd == null)
        {
            throw new KeyNotFoundException($"User with email '{request.MemberEmail}' is not registered in the system.");
        }

        if (circle.Members.Any(m => m.UserId == userToAdd.Id))
        {
            throw new InvalidOperationException($"User '{request.MemberEmail}' is already a member of this circle.");
        }

        var memberOrder = circle.Members.Count + 1;
        var member = new CircleMember
        {
            CircleId = circle.Id,
            UserId = userToAdd.Id,
            MemberOrder = memberOrder,
            RoleInCircle = CircleRole.Member,
            HasReceived = false,
            JoinedAt = DateTime.UtcNow
        };

        _context.CircleMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        return new CircleMemberDto
        {
            Id = member.Id,
            UserId = userToAdd.Id,
            FullName = userToAdd.FullName,
            Email = userToAdd.Email,
            MemberOrder = member.MemberOrder,
            RoleInCircle = member.RoleInCircle,
            HasReceived = member.HasReceived,
            JoinedAt = member.JoinedAt
        };
    }
}
