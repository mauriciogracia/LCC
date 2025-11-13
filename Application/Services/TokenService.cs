using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Domain.Interfaces;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly ILog log;

    public TokenService(IConfiguration config, ILog logger)
    {
        _config = config;
        log = logger;
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Uid),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("ReferralCode", user.ReferralCode ?? string.Empty)
        };
        var jwtKey = _config["Jwt:Key"];

        if (string.IsNullOrEmpty(jwtKey))
        {
            log.error("JWT Key is missing in configuration.");
            return string.Empty;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
