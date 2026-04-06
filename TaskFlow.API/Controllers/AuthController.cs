using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces.Security;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly TaskFlowDbContext _context;

    public AuthController(
        TaskFlowDbContext context,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var exists = await _context.Users.AnyAsync(x => x.Username == request.Username);
        if (exists)
            return Conflict("Bu kullanıcı adı zaten alınmış.");

        var user = new AppUser
        {
            Username = request.Username,
            PasswordHash = _passwordHasherService.Hash(request.Password),
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("Kayıt başarılı.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Username == request.Username);

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