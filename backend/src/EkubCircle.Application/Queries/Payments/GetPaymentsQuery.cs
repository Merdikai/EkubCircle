using MediatR;
using EkubCircle.Application.DTOs.Payments;

namespace EkubCircle.Application.Queries.Payments;

public record GetPaymentsQuery(int? CircleId, int? RoundId, int UserId) : IRequest<List<PaymentDto>>;
