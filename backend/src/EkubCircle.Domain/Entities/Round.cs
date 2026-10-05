using System.Text.Json.Serialization;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Domain.Entities;

public class Round
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public int RoundNumber { get; set; }
    public int ReceiverMemberId { get; set; }
    public string Status { get; set; } = RoundStatus.Pending;
    public decimal PotAmount { get; set; } = 0m;
    public DateTime? PaidOutAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Circle? Circle { get; set; }

    public CircleMember? ReceiverMember { get; set; }

    [JsonIgnore]
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
