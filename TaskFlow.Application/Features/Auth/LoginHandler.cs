using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Security;

namespace TaskFlow.Application.Features.Auth;

public class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<LoginResponseDto>> HandleAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);

        if (user is null)
            return Result<LoginResponseDto>.Failure(
                Error.Create(Error.Codes.Unauthorized, "Kullanıcı adı veya şifre hatalı."));

        var isValid = _passwordHasherService.Verify(password, user.PasswordHash);
        if (!isValid)
            return Result<LoginResponseDto>.Failure(
                Error.Create(Error.Codes.Unauthorized, "Kullanıcı adı veya şifre hatalı."));

        var token = _jwtTokenService.GenerateToken(user);

        return Result<LoginResponseDto>.Success(new LoginResponseDto
        {
            Token    = token,
            ExpireAt = DateTime.UtcNow.AddMinutes(60)
        });
    }
}
