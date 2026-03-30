using System.Security.Cryptography;
using System.Text;
using TaskFlow.Application.Interfaces.Security;

namespace TaskFlow.Infrastructure.Security;

public class PasswordHasherService : IPasswordHasherService
{
    public string Hash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool Verify(string password, string passwordHash)
    {
        var hashed = Hash(password);
        return hashed == passwordHash;
    }
}