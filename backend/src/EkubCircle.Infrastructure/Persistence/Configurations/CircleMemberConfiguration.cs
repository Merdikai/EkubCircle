using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EkubCircle.Domain.Entities;

namespace EkubCircle.Infrastructure.Persistence.Configurations;

public class CircleMemberConfiguration : IEntityTypeConfiguration<CircleMember>
{
    public void Configure(EntityTypeBuilder<CircleMember> builder)
    {
        builder.HasKey(cm => cm.Id);
        builder.HasIndex(cm => new { cm.CircleId, cm.UserId }).IsUnique();
        builder.Property(cm => cm.RoleInCircle).IsRequired().HasMaxLength(50);

        builder.Ignore(cm => cm.HasWon);

        builder.HasOne(cm => cm.Circle)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.CircleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.User)
            .WithMany(u => u.CircleMemberships)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
