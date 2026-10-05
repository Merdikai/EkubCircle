using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EkubCircle.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace EkubCircle.API.Services;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "EkubCircle_Super_Secret_Key_For_Hackathon_2026_Minimum_32_Bytes!";
        var jwtIssuer = _config["Jwt:Issuer"] ?? "EkubCircleAPI";
        var jwtAudience = _config["Jwt:Audience"] ?? "EkubCircleClient";
        var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var mins) ? mins : 1440;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
