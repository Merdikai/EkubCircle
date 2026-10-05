using System.Text.Json.Serialization;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Domain.Entities;

public class Circle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal ContributionAmount { get; set; }
    public string Frequency { get; set; } = CircleFrequency.Weekly;
    public string MeetingLabel { get => Frequency; set => Frequency = value; }
    public int MaxMembers { get; set; } = 10;
    public string Status { get; set; } = CircleStatus.Draft;
    public DateTime? StartDate { get; set; }
    public DateTime? StartedAt { get => StartDate; set => StartDate = value; }
    public DateTime? CompletedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? CreatedByUser { get; set; }
    public ICollection<CircleMember> Members { get; set; } = new List<CircleMember>();

    [JsonIgnore]
    public ICollection<Round> Rounds { get; set; } = new List<Round>();

    [JsonIgnore]
    public ICollection<JoinRequest> JoinRequests { get; set; } = new List<JoinRequest>();
}
