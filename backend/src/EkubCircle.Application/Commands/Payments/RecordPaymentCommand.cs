using MediatR;
using EkubCircle.Application.DTOs.Payments;

namespace EkubCircle.Application.Commands.Payments;

public record RecordPaymentCommand(
    int RecordedByUserId,
    int RoundId,
    int MemberId,
    decimal Amount,
    string PaymentMethod = "Cash",
    string? Notes = null,
    bool IsLate = false
) : IRequest<PaymentDto>;
