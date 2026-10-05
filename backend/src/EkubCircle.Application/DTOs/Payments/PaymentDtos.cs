namespace EkubCircle.Application.DTOs.Payments;

public class RecordPaymentRequestDto
{
    public int RoundId { get; set; }
    public int MemberId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
    public bool IsLate { get; set; } = false;
}

public class PaymentDto
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public int RoundNumber { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsLate { get; set; }
    public DateTime PaidAt { get; set; }
}
