using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces.Security;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;

    private readonly TaskFlowDbContext _context; // TEST-999
    // comment deneme

    public AuthController(
        TaskFlowDbContext context,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Username == request.Username);

        if (user is null)
            return Unauthorized("Kullanıcı adı veya şifre hatalı");

        
        var isValid = _passwordHasherService.Verify(request.Password, user.PasswordHash);

        if (!isValid)
            return Unauthorized("Kullanıcı adı veya şifre hatalı");

        
        var token = _jwtTokenService.GenerateToken(user);

        
        return Ok(new LoginResponseDto
        {
            Token = token,
            ExpireAt = DateTime.UtcNow.AddMinutes(60)
        });
    }
}