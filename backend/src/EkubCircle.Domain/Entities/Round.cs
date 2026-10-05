using System.Text.Json.Serialization;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Domain.Entities;

public class Round
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public int RoundNumber { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal PotAmount { get; set; } = 0m;
    public string Status { get; set; } = RoundStatus.Pending;
    public DateTime? DrawnAt { get; set; }
    public DateTime? PaidOutAt { get => DrawnAt; set => DrawnAt = value; }

    public int? WinnerMemberId { get; set; }

    public int ReceiverMemberId
    {
        get => WinnerMemberId ?? 0;
        set => WinnerMemberId = value > 0 ? value : null;
    }

    // Navigation properties
    [JsonIgnore]
    public Circle? Circle { get; set; }

    public CircleMember? WinnerMember { get; set; }
    public CircleMember? ReceiverMember
    {
        get => WinnerMember;
        set => WinnerMember = value;
    }

    [JsonIgnore]
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
