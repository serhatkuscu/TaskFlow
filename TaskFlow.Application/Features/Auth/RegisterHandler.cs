using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Security;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Auth;

public class RegisterHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;

    public RegisterHandler(IUserRepository userRepository, IPasswordHasherService passwordHasherService)
    {
        _userRepository        = userRepository;
        _passwordHasherService = passwordHasherService;
    }

    public async Task<Result<string>> HandleAsync(string username, string password)
    {
        var exists = await _userRepository.ExistsAsync(username);
        if (exists)
            return Result<string>.Failure(
                Error.Create(Error.Codes.Conflict, "Bu kullanıcı adı zaten alınmış."));

        var user = new AppUser
        {
            Username     = username,
            PasswordHash = _passwordHasherService.Hash(password),
            Role         = AppUserRoles.User
        };

        await _userRepository.AddAsync(user);

        return Result<string>.Success("Kayıt başarılı.");
    }
}
