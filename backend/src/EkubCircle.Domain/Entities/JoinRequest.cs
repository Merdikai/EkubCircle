using System.Text.Json.Serialization;
using EkubCircle.Domain.Enums;

namespace EkubCircle.Domain.Entities;

public class JoinRequest
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public int RequestedUserId { get; set; }
    public int RequestedByUserId { get; set; }
    public string Status { get; set; } = JoinRequestStatus.Pending;
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Circle? Circle { get; set; }

    public User? RequestedUser { get; set; }
    public User? RequestedByUser { get; set; }
}
