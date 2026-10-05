using EkubCircle.Domain.Entities;

namespace EkubCircle.Application.Common.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
