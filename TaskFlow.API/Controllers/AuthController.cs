using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Security;
using TaskFlow.Domain.Entities;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        IUserRepository userRepository,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var exists = await _userRepository.ExistsAsync(request.Username);
        if (exists)
            return Conflict("Bu kullanıcı adı zaten alınmış.");

        var user = new AppUser
        {
            Username = request.Username,
            PasswordHash = _passwordHasherService.Hash(request.Password),
            Role = "User"
        };

        await _userRepository.AddAsync(user);

        return Ok("Kayıt başarılı.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user is null)
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        var isValid = _passwordHasherService.Verify(request.Password, user.PasswordHash);

        if (!isValid)
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        var token = _jwtTokenService.GenerateToken(user);

        return Ok(new LoginResponseDto
        {
            Token = token,
            ExpireAt = DateTime.UtcNow.AddMinutes(60)
        });
    }
}
