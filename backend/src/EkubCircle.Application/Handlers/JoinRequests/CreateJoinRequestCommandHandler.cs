using MediatR;
using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Commands.JoinRequests;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.DTOs.JoinRequests;
using EkubCircle.Application.Exceptions;
using EkubCircle.Domain.Entities;
using EkubCircle.Domain.Enums;
using EkubCircle.Domain.Exceptions;

namespace EkubCircle.Application.Handlers.JoinRequests;

public class CreateJoinRequestCommandHandler : IRequestHandler<CreateJoinRequestCommand, JoinRequestDto>
{
    private readonly IEkubDbContext _context;

    public CreateJoinRequestCommandHandler(IEkubDbContext context)
    {
        _context = context;
    }

    public async Task<JoinRequestDto> Handle(CreateJoinRequestCommand request, CancellationToken cancellationToken)
    {
        var circle = await _context.Circles
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == request.CircleId, cancellationToken);

        if (circle == null)
        {
            throw new NotFoundException($"Circle with ID {request.CircleId} not found.");
        }

        if (circle.Status != CircleStatus.Forming && circle.Status != CircleStatus.Draft)
        {
            throw new EkubRuleViolationException("Cannot request to join or invite members to a circle that is already Active or Completed.");
        }

        if (circle.Members.Count >= circle.MaxMembers)
        {
            throw new EkubRuleViolationException($"Circle has already reached its maximum capacity of {circle.MaxMembers} members.");
        }

        int targetUserId = request.CurrentUserId;
        if (request.RequestedUserId.HasValue && request.RequestedUserId.Value > 0)
        {
            targetUserId = request.RequestedUserId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var userByEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);

            if (userByEmail == null)
            {
                throw new NotFoundException($"User with email '{request.Email}' not found.");
            }
            targetUserId = userByEmail.Id;
        }

        var targetUser = await _context.Users.FindAsync(new object[] { targetUserId }, cancellationToken);
        if (targetUser == null)
        {
            throw new NotFoundException($"User with ID {targetUserId} not found.");
        }

        var currentUser = await _context.Users.FindAsync(new object[] { request.CurrentUserId }, cancellationToken);

        // Check if already a member
        if (circle.Members.Any(m => m.UserId == targetUserId))
        {
            throw new EkubRuleViolationException("User is already a member of this circle.");
        }

        // Check if an existing pending request exists
        var existingPending = await _context.JoinRequests
            .AnyAsync(jr => jr.CircleId == circle.Id && jr.RequestedUserId == targetUserId && jr.Status == JoinRequestStatus.Pending, cancellationToken);

        if (existingPending)
        {
            throw new EkubRuleViolationException("A pending join request or invitation already exists for this user in this circle.");
        }

        var joinRequest = new JoinRequest
        {
            CircleId = circle.Id,
            RequestedUserId = targetUserId,
            RequestedByUserId = request.CurrentUserId,
            Status = JoinRequestStatus.Pending,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow
        };

        _context.JoinRequests.Add(joinRequest);

        // Create notification
        if (targetUserId == request.CurrentUserId)
        {
            // Member initiated request -> notify Organizer
            var organizer = circle.Members.FirstOrDefault(m => m.RoleInCircle == CircleRole.Organizer);
            int notifyUserId = organizer?.UserId ?? circle.CreatedByUserId;

            _context.Notifications.Add(new Notification
            {
                UserId = notifyUserId,
                Type = "JoinRequest",
                Title = "New Join Request",
                Message = $"{currentUser?.FullName ?? "A user"} requested to join '{circle.Name}'.",
                RelatedEntityId = circle.Id,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            // Organizer initiated invitation -> notify Target User
            _context.Notifications.Add(new Notification
            {
                UserId = targetUserId,
                Type = "CircleInvitation",
                Title = "Circle Invitation",
                Message = $"You have been invited to join '{circle.Name}' by {currentUser?.FullName ?? "an organizer"}.",
                RelatedEntityId = circle.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new JoinRequestDto
        {
            Id = joinRequest.Id,
            CircleId = circle.Id,
            CircleName = circle.Name,
            RequestedUserId = targetUser.Id,
            RequestedUserName = targetUser.FullName,
            RequestedUserEmail = targetUser.Email,
            RequestedByUserId = request.CurrentUserId,
            RequestedByUserName = currentUser?.FullName ?? string.Empty,
            Status = joinRequest.Status,
            Message = joinRequest.Message,
            CreatedAt = joinRequest.CreatedAt,
            RespondedAt = joinRequest.RespondedAt
        };
    }
}
