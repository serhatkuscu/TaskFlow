using TaskFlow.Application.Interfaces.Security;

namespace TaskFlow.Infrastructure.Security;

public class PasswordHasherService : IPasswordHasherService
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
