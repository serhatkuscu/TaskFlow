using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Security;

namespace TaskFlow.Application.Features.Auth;

public class LoginHandler
{
    private readonly IUserRepository          _userRepository;
    private readonly IPasswordHasherService   _passwordHasherService;
    private readonly IJwtTokenService         _jwtTokenService;
    private readonly ILogger<LoginHandler>    _logger;

    public LoginHandler(
        IUserRepository        userRepository,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService       jwtTokenService,
        ILogger<LoginHandler>  logger)
    {
        _userRepository        = userRepository;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService       = jwtTokenService;
        _logger                = logger;
    }

    public async Task<Result<LoginResponseDto>> HandleAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);

        if (user is null)
        {
            // Log the username but NEVER log the password.
            _logger.LogWarning("Login failed: user not found. Username: {Username}", username);
            return Result<LoginResponseDto>.Failure(
                Error.Create(Error.Codes.Unauthorized, "Kullanıcı adı veya şifre hatalı."));
        }

        var isValid = _passwordHasherService.Verify(password, user.PasswordHash);
        if (!isValid)
        {
            _logger.LogWarning("Login failed: invalid password. Username: {Username}", username);
            return Result<LoginResponseDto>.Failure(
                Error.Create(Error.Codes.Unauthorized, "Kullanıcı adı veya şifre hatalı."));
        }

        var tokenResult = _jwtTokenService.GenerateToken(user);

        // Log the expiry but NEVER log the token itself.
        _logger.LogInformation(
            "Login successful. Username: {Username}, TokenExpiresAt: {ExpireAt}",
            username,
            tokenResult.ExpireAt);

        return Result<LoginResponseDto>.Success(new LoginResponseDto
        {
            Token    = tokenResult.Token,
            ExpireAt = tokenResult.ExpireAt
        });
    }
}
