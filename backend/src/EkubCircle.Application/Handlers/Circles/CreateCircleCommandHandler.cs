using MediatR;
using EkubCircle.Application.Commands.Circles;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Application.DTOs.Circles;
using EkubCircle.Domain.Entities;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Application.Handlers.Circles;

public class CreateCircleCommandHandler : IRequestHandler<CreateCircleCommand, CircleDetailDto>
{
    private readonly IEkubDbContext _context;

    public CreateCircleCommandHandler(IEkubDbContext context)
    {
        _context = context;
    }

    public async Task<CircleDetailDto> Handle(CreateCircleCommand request, CancellationToken cancellationToken)
    {
        if (request.ContributionAmount <= 0)
        {
            throw new ArgumentException("Contribution amount must be greater than zero.");
        }

        var circle = new Circle
        {
            Name = request.Name.Trim(),
            ContributionAmount = request.ContributionAmount,
            MeetingLabel = string.IsNullOrWhiteSpace(request.MeetingLabel) ? "Weekly" : request.MeetingLabel.Trim(),
            Status = CircleStatus.Forming,
            CreatedByUserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        var organizerMember = new CircleMember
        {
            Circle = circle,
            UserId = request.UserId,
            MemberOrder = 1,
            RoleInCircle = CircleRole.Organizer,
            HasReceived = false,
            JoinedAt = DateTime.UtcNow
        };

        circle.Members.Add(organizerMember);

        _context.Circles.Add(circle);
        await _context.SaveChangesAsync(cancellationToken);

        // Fetch user info for DTO
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        return new CircleDetailDto
        {
            Id = circle.Id,
            Name = circle.Name,
            ContributionAmount = circle.ContributionAmount,
            MeetingLabel = circle.MeetingLabel,
            Status = circle.Status,
            CreatedByUserId = circle.CreatedByUserId,
            CreatedByUserName = user?.FullName ?? string.Empty,
            CreatedAt = circle.CreatedAt,
            StartedAt = circle.StartedAt,
            CompletedAt = circle.CompletedAt,
            MemberCount = circle.Members.Count,
            TotalRounds = 0,
            CurrentRoundNumber = 0,
            Members = new List<CircleMemberDto>
            {
                new CircleMemberDto
                {
                    Id = organizerMember.Id,
                    UserId = organizerMember.UserId,
                    FullName = user?.FullName ?? string.Empty,
                    Email = user?.Email ?? string.Empty,
                    MemberOrder = organizerMember.MemberOrder,
                    RoleInCircle = organizerMember.RoleInCircle,
                    HasReceived = organizerMember.HasReceived,
                    JoinedAt = organizerMember.JoinedAt
                }
            }
        };
    }
}
