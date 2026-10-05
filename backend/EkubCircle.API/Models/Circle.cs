using System.Text.Json.Serialization;

namespace EkubCircle.API.Models;

public static class CircleStatus
{
    public const string Forming = "Forming";
    public const string Active = "Active";
    public const string Completed = "Completed";
}

public class Circle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal ContributionAmount { get; set; }
    public string MeetingLabel { get; set; } = "Weekly"; // "Weekly", "Monthly", etc.
    public string Status { get; set; } = CircleStatus.Forming;

    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public User? CreatedByUser { get; set; }
    public ICollection<CircleMember> Members { get; set; } = new List<CircleMember>();
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}
