using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces.Security;

public interface IJwtTokenService
{
    string GenerateToken(AppUser user);
}