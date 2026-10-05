using EkubCircle.API.Models;

namespace EkubCircle.API.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
