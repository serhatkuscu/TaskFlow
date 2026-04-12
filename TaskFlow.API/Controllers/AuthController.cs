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
    private readonly LoginHandler    _loginHandler;

    public AuthController(RegisterHandler registerHandler, LoginHandler loginHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler    = loginHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _registerHandler.HandleAsync(request.Username, request.Password);

        return result.IsFailure
            ? ToErrorResponse(result.Error!)
            : StatusCode(201, new { message = result.Value });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _loginHandler.HandleAsync(request.Username, request.Password);

        return result.IsFailure
            ? ToErrorResponse(result.Error!)
            : Ok(result.Value);
    }

    private IActionResult ToErrorResponse(Error error)
    {
        var body = new { code = error.Code, message = error.Message };

        return error.Code switch
        {
            Error.Codes.Conflict     => Conflict(body),
            Error.Codes.Unauthorized => Unauthorized(body),
            Error.Codes.NotFound     => NotFound(body),
            Error.Codes.Validation   => BadRequest(body),
            _                        => StatusCode(500, body)
        };
    }
}
