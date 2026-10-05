using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EkubCircle.Domain.Entities;

namespace EkubCircle.Infrastructure.Persistence.Configurations;

public class JoinRequestConfiguration : IEntityTypeConfiguration<JoinRequest>
{
    public void Configure(EntityTypeBuilder<JoinRequest> builder)
    {
        builder.HasKey(jr => jr.Id);
        builder.Property(jr => jr.Status).IsRequired().HasMaxLength(50);
        builder.Property(jr => jr.Message).HasMaxLength(500);

        builder.HasOne(jr => jr.Circle)
            .WithMany(c => c.JoinRequests)
            .HasForeignKey(jr => jr.CircleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(jr => jr.RequestedUser)
            .WithMany(u => u.ReceivedJoinRequests)
            .HasForeignKey(jr => jr.RequestedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(jr => jr.RequestedByUser)
            .WithMany(u => u.SentJoinRequests)
            .HasForeignKey(jr => jr.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(jr => new { jr.CircleId, jr.RequestedUserId });
    }
}
