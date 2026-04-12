using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Features.Auth;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterHandler _registerHandler;
    private readonly LoginHandler _loginHandler;

    public AuthController(RegisterHandler registerHandler, LoginHandler loginHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler    = loginHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _registerHandler.HandleAsync(request.Username, request.Password);

        if (result.IsFailure)
            return Conflict(new { code = result.Error!.Code, message = result.Error.Message });

        return Ok(new { message = result.Value });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _loginHandler.HandleAsync(request.Username, request.Password);

        if (result.IsFailure)
            return Unauthorized(new { code = result.Error!.Code, message = result.Error.Message });

        return Ok(result.Value);
    }
}
