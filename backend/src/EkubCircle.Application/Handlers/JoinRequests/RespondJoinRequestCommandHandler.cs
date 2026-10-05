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

public class RespondJoinRequestCommandHandler : IRequestHandler<RespondJoinRequestCommand, JoinRequestDto>
{
    private readonly IEkubDbContext _context;

    public RespondJoinRequestCommandHandler(IEkubDbContext context)
    {
        _context = context;
    }

    public async Task<JoinRequestDto> Handle(RespondJoinRequestCommand request, CancellationToken cancellationToken)
    {
        var joinRequest = await _context.JoinRequests
            .Include(jr => jr.Circle)
                .ThenInclude(c => c!.Members)
            .Include(jr => jr.RequestedUser)
            .Include(jr => jr.RequestedByUser)
            .FirstOrDefaultAsync(jr => jr.Id == request.RequestId, cancellationToken);

        if (joinRequest == null)
        {
            throw new NotFoundException($"Join request with ID {request.RequestId} not found.");
        }

        if (joinRequest.Status != JoinRequestStatus.Pending)
        {
            throw new EkubRuleViolationException($"Join request is already {joinRequest.Status}.");
        }

        var circle = joinRequest.Circle;
        if (circle == null)
        {
            throw new NotFoundException("Circle not found.");
        }

        if (circle.Status != CircleStatus.Forming && circle.Status != CircleStatus.Draft)
        {
            throw new EkubRuleViolationException("Cannot respond to join request for a circle that is already Active or Completed.");
        }

        bool isAccepted = request.Status.Equals("Accepted", StringComparison.OrdinalIgnoreCase);

        if (isAccepted)
        {
            if (circle.Members.Count >= circle.MaxMembers)
            {
                throw new EkubRuleViolationException($"Circle has already reached its maximum capacity of {circle.MaxMembers} members.");
            }

            // Check if already in circle
            if (!circle.Members.Any(m => m.UserId == joinRequest.RequestedUserId))
            {
                var newMember = new CircleMember
                {
                    CircleId = circle.Id,
                    UserId = joinRequest.RequestedUserId,
                    RoleInCircle = CircleRole.Member,
                    MemberOrder = 0,
                    HasReceived = false,
                    JoinedAt = DateTime.UtcNow
                };

                _context.CircleMembers.Add(newMember);
            }

            joinRequest.Status = JoinRequestStatus.Accepted;

            // Notify user
            _context.Notifications.Add(new Notification
            {
                UserId = joinRequest.RequestedUserId,
                Type = "JoinRequestAccepted",
                Title = "Join Request Accepted",
                Message = $"Your request to join '{circle.Name}' has been accepted!",
                RelatedEntityId = circle.Id,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            joinRequest.Status = JoinRequestStatus.Rejected;

            _context.Notifications.Add(new Notification
            {
                UserId = joinRequest.RequestedUserId,
                Type = "JoinRequestRejected",
                Title = "Join Request Declined",
                Message = $"Your request to join '{circle.Name}' was not accepted.",
                RelatedEntityId = circle.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        joinRequest.RespondedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new JoinRequestDto
        {
            Id = joinRequest.Id,
            CircleId = circle.Id,
            CircleName = circle.Name,
            RequestedUserId = joinRequest.RequestedUserId,
            RequestedUserName = joinRequest.RequestedUser?.FullName ?? string.Empty,
            RequestedUserEmail = joinRequest.RequestedUser?.Email ?? string.Empty,
            RequestedByUserId = joinRequest.RequestedByUserId,
            RequestedByUserName = joinRequest.RequestedByUser?.FullName ?? string.Empty,
            Status = joinRequest.Status,
            Message = joinRequest.Message,
            CreatedAt = joinRequest.CreatedAt,
            RespondedAt = joinRequest.RespondedAt
        };
    }
}
