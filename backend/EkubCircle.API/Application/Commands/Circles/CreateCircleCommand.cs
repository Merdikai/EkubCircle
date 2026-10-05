using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Circles;
using EkubCircle.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Commands.Circles;

public record CreateCircleCommand(int UserId, string Name, decimal ContributionAmount, string MeetingLabel) : IRequest<CircleDetailDto>;

public class CreateCircleCommandHandler : IRequestHandler<CreateCircleCommand, CircleDetailDto>
{
    private readonly EkubDbContext _context;

    public CreateCircleCommandHandler(EkubDbContext context)
    {
        _context = context;
    }

    public async Task<CircleDetailDto> Handle(CreateCircleCommand request, CancellationToken cancellationToken)
    {
        if (request.ContributionAmount <= 0)
        {
            throw new ArgumentException("Contribution amount must be greater than zero.");
        }

        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken)
                   ?? throw new KeyNotFoundException("Creator user not found.");

        var circle = new Circle
        {
            Name = request.Name.Trim(),
            ContributionAmount = request.ContributionAmount,
            MeetingLabel = string.IsNullOrWhiteSpace(request.MeetingLabel) ? "Weekly" : request.MeetingLabel.Trim(),
            Status = CircleStatus.Forming,
            CreatedByUserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Circles.Add(circle);
        await _context.SaveChangesAsync(cancellationToken);

        // Creator automatically joins as Organizer
        var organizerMember = new CircleMember
        {
            CircleId = circle.Id,
            UserId = request.UserId,
            MemberOrder = 0,
            RoleInCircle = CircleRole.Organizer,
            HasReceived = false,
            JoinedAt = DateTime.UtcNow
        };

        _context.CircleMembers.Add(organizerMember);
        await _context.SaveChangesAsync(cancellationToken);

        return new CircleDetailDto
        {
            Id = circle.Id,
            Name = circle.Name,
            ContributionAmount = circle.ContributionAmount,
            MeetingLabel = circle.MeetingLabel,
            Status = circle.Status,
            CreatedByUserId = circle.CreatedByUserId,
            CreatedByUserName = user.FullName,
            CreatedAt = circle.CreatedAt,
            StartedAt = circle.StartedAt,
            CompletedAt = circle.CompletedAt,
            MemberCount = 1,
            TotalRounds = 0,
            CurrentRoundNumber = 0,
            Members = new List<CircleMemberDto>
            {
                new CircleMemberDto
                {
                    Id = organizerMember.Id,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    MemberOrder = 0,
                    RoleInCircle = organizerMember.RoleInCircle,
                    HasReceived = false,
                    JoinedAt = organizerMember.JoinedAt
                }
            }
        };
    }
}
