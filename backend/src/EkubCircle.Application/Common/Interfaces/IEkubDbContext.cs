using Microsoft.EntityFrameworkCore;
using EkubCircle.Domain.Entities;

namespace EkubCircle.Application.Common.Interfaces;

public interface IEkubDbContext
{
    DbSet<User> Users { get; }
    DbSet<Circle> Circles { get; }
    DbSet<CircleMember> CircleMembers { get; }
    DbSet<Round> Rounds { get; }
    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
