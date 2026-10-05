using System.Text.Json.Serialization;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Name { get => FullName; set => FullName = value; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonIgnore]
    public ICollection<Circle> CreatedCircles { get; set; } = new List<Circle>();

    [JsonIgnore]
    public ICollection<CircleMember> CircleMemberships { get; set; } = new List<CircleMember>();

    [JsonIgnore]
    public ICollection<Payment> RecordedPayments { get; set; } = new List<Payment>();

    [JsonIgnore]
    public ICollection<JoinRequest> ReceivedJoinRequests { get; set; } = new List<JoinRequest>();

    [JsonIgnore]
    public ICollection<JoinRequest> SentJoinRequests { get; set; } = new List<JoinRequest>();

    [JsonIgnore]
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
