namespace EkubCircle.Application.DTOs.Circles;

public class CreateCircleRequestDto
{
    public string Name { get; set; } = string.Empty;
    public decimal ContributionAmount { get; set; }
    public string MeetingLabel { get; set; } = "Weekly";
}

public class AddMemberRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class CircleMemberDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int MemberOrder { get; set; }
    public string RoleInCircle { get; set; } = string.Empty;
    public bool HasReceived { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class CircleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal ContributionAmount { get; set; }
    public string MeetingLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int MemberCount { get; set; }
}

public class CircleDetailDto : CircleDto
{
    public List<CircleMemberDto> Members { get; set; } = new();
    public int TotalRounds { get; set; }
    public int CurrentRoundNumber { get; set; }
}
