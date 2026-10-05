using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EkubCircle.Domain.Entities;

namespace EkubCircle.Infrastructure.Persistence.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.CircleId, r.RoundNumber }).IsUnique();
        builder.Property(r => r.Status).IsRequired().HasMaxLength(50);
        builder.Property(r => r.PotAmount).HasPrecision(18, 2);

        builder.HasOne(r => r.Circle)
            .WithMany(c => c.Rounds)
            .HasForeignKey(r => r.CircleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReceiverMember)
            .WithMany(cm => cm.ReceivedRounds)
            .HasForeignKey(r => r.ReceiverMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
