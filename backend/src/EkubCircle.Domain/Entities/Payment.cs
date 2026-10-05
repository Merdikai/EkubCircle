using System.Text.Json.Serialization;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public int CircleMemberId { get; set; }
    public int MemberId { get => CircleMemberId; set => CircleMemberId = value; }
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = Enums.PaymentType.Normal;
    public int ChanceCount { get; set; } = 1;
    public string Status { get; set; } = PaymentStatus.Paid;
    public string PaymentMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
    public bool IsLate { get; set; } = false;
    public DateTime? PaidAt { get; set; } = DateTime.UtcNow;
    public int RecordedByUserId { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Round? Round { get; set; }

    public CircleMember? Member { get; set; }
    public User? RecordedByUser { get; set; }
}
