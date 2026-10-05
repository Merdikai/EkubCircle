using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EkubCircle.Domain.Entities;

namespace EkubCircle.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.RoundId, p.MemberId, p.PaymentType }).IsUnique();
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.PaymentType).IsRequired().HasMaxLength(50);
        builder.Property(p => p.PaymentMethod).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Notes).HasMaxLength(500);

        builder.HasOne(p => p.Round)
            .WithMany(r => r.Payments)
            .HasForeignKey(p => p.RoundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Member)
            .WithMany(cm => cm.Payments)
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.RecordedByUser)
            .WithMany(u => u.RecordedPayments)
            .HasForeignKey(p => p.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
