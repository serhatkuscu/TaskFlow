using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.Interfaces.Security;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public JwtTokenResult GenerateToken(AppUser user)
    {
        var jwtSettings    = _configuration.GetSection("Jwt");
        var key            = jwtSettings["Key"]!;
        var issuer         = jwtSettings["Issuer"]!;
        var audience       = jwtSettings["Audience"]!;
        var expireMinutes  = int.Parse(jwtSettings["ExpireMinutes"]!);
        var expireAt       = DateTime.UtcNow.AddMinutes(expireMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Username),
            new(ClaimTypes.Role,           user.Role)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:            issuer,
            audience:          audience,
            claims:            claims,
            expires:           expireAt,
            signingCredentials: credentials
        );

        return new JwtTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expireAt);
    }
}
