using System.Text.Json.Serialization;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Domain.Entities;

public class CircleMember
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public int UserId { get; set; }
    public int MemberOrder { get; set; }
    public string RoleInCircle { get; set; } = CircleRole.Member;
    public bool HasReceived { get; set; } = false;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonIgnore]
    public Circle? Circle { get; set; }

    public User? User { get; set; }

    [JsonIgnore]
    public ICollection<Round> ReceivedRounds { get; set; } = new List<Round>();

    [JsonIgnore]
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
