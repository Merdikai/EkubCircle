using System.Text.Json.Serialization;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public int MemberId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = Enums.PaymentType.Contribution;
    public string PaymentMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public int RecordedByUserId { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Round? Round { get; set; }

    public CircleMember? Member { get; set; }
    public User? RecordedByUser { get; set; }
}
