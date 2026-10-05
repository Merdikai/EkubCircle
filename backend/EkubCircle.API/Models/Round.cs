using System.Text.Json.Serialization;

namespace EkubCircle.API.Models;

public static class RoundStatus
{
    public const string Open = "Open";
    public const string PaidOut = "PaidOut";
}

public class Round
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public int RoundNumber { get; set; }
    public int ReceiverMemberId { get; set; }
    public string Status { get; set; } = RoundStatus.Open;
    public decimal PotAmount { get; set; } = 0;
    public DateTime? PaidOutAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Circle? Circle { get; set; }

    public CircleMember? ReceiverMember { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
