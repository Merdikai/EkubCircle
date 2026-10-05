using Microsoft.EntityFrameworkCore;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Domain.Entities;

namespace EkubCircle.Infrastructure.Persistence.Context;

public class EkubDbContext : DbContext, IEkubDbContext
{
    public EkubDbContext(DbContextOptions<EkubDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Circle> Circles => Set<Circle>();
    public DbSet<CircleMember> CircleMembers => Set<CircleMember>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EkubDbContext).Assembly);
    }
}
