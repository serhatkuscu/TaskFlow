using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces.Security;

public record JwtTokenResult(string Token, DateTime ExpireAt);

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(AppUser user);
}
