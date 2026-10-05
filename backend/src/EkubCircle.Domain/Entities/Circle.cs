using System.Text.Json.Serialization;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Domain.Entities;

public class Circle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal ContributionAmount { get; set; }
    public string MeetingLabel { get; set; } = "Weekly";
    public string Status { get; set; } = CircleStatus.Forming;
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public User? CreatedByUser { get; set; }
    public ICollection<CircleMember> Members { get; set; } = new List<CircleMember>();

    [JsonIgnore]
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}
