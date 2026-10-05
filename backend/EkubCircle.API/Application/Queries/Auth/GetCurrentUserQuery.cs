using EkubCircle.API.Data;
using EkubCircle.API.DTOs.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Application.Queries.Auth;

public record GetCurrentUserQuery(int UserId) : IRequest<UserDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly EkubDbContext _context;

    public GetCurrentUserQueryHandler(EkubDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
}
