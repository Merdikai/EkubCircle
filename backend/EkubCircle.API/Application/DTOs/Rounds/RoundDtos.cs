namespace EkubCircle.API.DTOs.Rounds;

public class CurrentRoundMemberDto
{
    public int MemberId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int MemberOrder { get; set; }
    public bool HasReceived { get; set; }
    public bool HasPaidThisRound { get; set; }
    public decimal? AmountPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
}

public class CurrentRoundDto
{
    public int RoundId { get; set; }
    public int CircleId { get; set; }
    public string CircleName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public int TotalRounds { get; set; }
    public string Status { get; set; } = string.Empty; // Open, PaidOut, Pending
    public int ReceiverMemberId { get; set; }
    public string ReceiverFullName { get; set; } = string.Empty;
    public string ReceiverEmail { get; set; } = string.Empty;
    public int ReceiverMemberOrder { get; set; }
    public decimal ContributionAmount { get; set; }
    public decimal TargetPotAmount { get; set; }
    public decimal CurrentPotAmount { get; set; }
    public int TotalMembers { get; set; }
    public int PaidCount { get; set; }
    public bool IsReadyForPayout { get; set; }
    public List<CurrentRoundMemberDto> Members { get; set; } = new();
}

public class PayoutResultDto
{
    public int RoundId { get; set; }
    public int RoundNumber { get; set; }
    public decimal PotAmount { get; set; }
    public int ReceiverMemberId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public DateTime PaidOutAt { get; set; }
    public string RoundStatus { get; set; } = string.Empty;
    public string CircleStatus { get; set; } = string.Empty;
    public int? NextRoundNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class RoundSummaryDto
{
    public int RoundId { get; set; }
    public int RoundNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ReceiverMemberId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public decimal PotAmount { get; set; }
    public DateTime? PaidOutAt { get; set; }
    public int PaidCount { get; set; }
    public int TotalMembers { get; set; }
}
