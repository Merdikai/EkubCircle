using System.Text.Json.Serialization;

namespace EkubCircle.API.Models;

public static class PaymentType
{
    public const string Normal = "Normal";
    public const string Contribution = "Normal";
    public const string Extra = "Extra";
    public const string Payout = "Payout";
}

public class Payment
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public int MemberId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = Models.PaymentType.Normal;
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
