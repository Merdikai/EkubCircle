namespace EkubCircle.Application.DTOs.JoinRequests;

public class CreateJoinRequestDto
{
    public int CircleId { get; set; }
    public int? RequestedUserId { get; set; }
    public string? Email { get; set; }
    public string? Message { get; set; }
}

public class RespondJoinRequestDto
{
    public string Status { get; set; } = "Accepted"; // "Accepted" or "Rejected"
}

public class JoinRequestDto
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public string CircleName { get; set; } = string.Empty;
    public int RequestedUserId { get; set; }
    public string RequestedUserName { get; set; } = string.Empty;
    public string RequestedUserEmail { get; set; } = string.Empty;
    public int RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}
