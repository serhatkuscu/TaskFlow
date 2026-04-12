using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Security;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Auth;

public class RegisterHandler
{
    private readonly IUserRepository        _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        IUserRepository         userRepository,
        IPasswordHasherService  passwordHasherService,
        ILogger<RegisterHandler> logger)
    {
        _userRepository        = userRepository;
        _passwordHasherService = passwordHasherService;
        _logger                = logger;
    }

    public async Task<Result<string>> HandleAsync(string username, string password)
    {
        // Fast-path: avoids a write attempt in the common duplicate case.
        // This check is NOT the safety net — the DB unique constraint is.
        var exists = await _userRepository.ExistsAsync(username);
        if (exists)
        {
            _logger.LogWarning("Registration failed: username already taken. Username: {Username}", username);
            return Result<string>.Failure(
                Error.Create(Error.Codes.Conflict, "Bu kullanıcı adı zaten alınmış."));
        }

        var user = new AppUser
        {
            Username     = username,
            PasswordHash = _passwordHasherService.Hash(password),
            Role         = AppUserRoles.User
        };

        // AddAsync returns false when a concurrent registration wins the race and
        // the DB unique index blocks our insert. Treat it as a conflict, not a crash.
        var added = await _userRepository.AddAsync(user);
        if (!added)
        {
            _logger.LogWarning(
                "Registration failed: concurrent duplicate detected by DB constraint. Username: {Username}",
                username);
            return Result<string>.Failure(
                Error.Create(Error.Codes.Conflict, "Bu kullanıcı adı zaten alınmış."));
        }

        _logger.LogInformation("User registered successfully. Username: {Username}", username);
        return Result<string>.Success("Kayıt başarılı.");
    }
}
