using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EkubCircle.Domain.Entities;

namespace EkubCircle.Infrastructure.Persistence.Configurations;

public class CircleConfiguration : IEntityTypeConfiguration<Circle>
{
    public void Configure(EntityTypeBuilder<Circle> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.ContributionAmount).HasPrecision(18, 2);
        builder.Property(c => c.Frequency).IsRequired().HasMaxLength(50);
        builder.Property(c => c.MaxMembers).HasDefaultValue(10);
        builder.Property(c => c.Status).IsRequired().HasMaxLength(50);
        builder.Property(c => c.StartDate);

        builder.Ignore(c => c.MeetingLabel);
        builder.Ignore(c => c.StartedAt);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany(u => u.CreatedCircles)
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
