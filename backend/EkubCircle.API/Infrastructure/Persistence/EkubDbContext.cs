using EkubCircle.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EkubCircle.API.Data;

public class EkubDbContext : DbContext
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

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
        });

        // Circle
        modelBuilder.Entity<Circle>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.ContributionAmount).HasPrecision(18, 2);
            entity.Property(c => c.MeetingLabel).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Status).IsRequired().HasMaxLength(50);

            entity.HasOne(c => c.CreatedByUser)
                  .WithMany(u => u.CreatedCircles)
                  .HasForeignKey(c => c.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // CircleMember
        modelBuilder.Entity<CircleMember>(entity =>
        {
            entity.HasKey(cm => cm.Id);
            entity.HasIndex(cm => new { cm.CircleId, cm.UserId }).IsUnique();

            entity.Property(cm => cm.RoleInCircle).IsRequired().HasMaxLength(50);

            entity.HasOne(cm => cm.Circle)
                  .WithMany(c => c.Members)
                  .HasForeignKey(cm => cm.CircleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cm => cm.User)
                  .WithMany(u => u.CircleMemberships)
                  .HasForeignKey(cm => cm.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Round
        modelBuilder.Entity<Round>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => new { r.CircleId, r.RoundNumber }).IsUnique();
            entity.Property(r => r.Status).IsRequired().HasMaxLength(50);
            entity.Property(r => r.PotAmount).HasPrecision(18, 2);

            entity.HasOne(r => r.Circle)
                  .WithMany(c => c.Rounds)
                  .HasForeignKey(r => r.CircleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.ReceiverMember)
                  .WithMany(cm => cm.ReceivedRounds)
                  .HasForeignKey(r => r.ReceiverMemberId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => new { p.RoundId, p.MemberId, p.PaymentType }).IsUnique();
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.Property(p => p.PaymentType).IsRequired().HasMaxLength(50);

            entity.HasOne(p => p.Round)
                  .WithMany(r => r.Payments)
                  .HasForeignKey(p => p.RoundId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Member)
                  .WithMany(cm => cm.Payments)
                  .HasForeignKey(p => p.MemberId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.RecordedByUser)
                  .WithMany(u => u.RecordedPayments)
                  .HasForeignKey(p => p.RecordedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
