using System.Text.Json.Serialization;

namespace EkubCircle.API.Models;

public static class CircleRole
{
    public const string Organizer = "Organizer";
    public const string Member = "Member";
}

public class CircleMember
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public int UserId { get; set; }

    public int MemberOrder { get; set; } = 0; // 1..N assigned deterministically on Start
    public string RoleInCircle { get; set; } = CircleRole.Member; // "Organizer", "Member"
    public bool HasReceived { get; set; } = false; // Pot receipt flag
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
