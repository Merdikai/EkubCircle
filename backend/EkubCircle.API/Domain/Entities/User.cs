using System.Text.Json.Serialization;

namespace EkubCircle.API.Models;

public static class UserRole
{
    public const string Admin = "Admin";
    public const string User = "User";
}

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "User"; // "Admin", "User"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonIgnore]
    public ICollection<Circle> CreatedCircles { get; set; } = new List<Circle>();

    [JsonIgnore]
    public ICollection<CircleMember> CircleMemberships { get; set; } = new List<CircleMember>();

    [JsonIgnore]
    public ICollection<Payment> RecordedPayments { get; set; } = new List<Payment>();
}
