using EkubCircle.Domain.Entities;

namespace EkubCircle.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
