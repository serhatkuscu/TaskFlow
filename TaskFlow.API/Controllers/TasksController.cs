using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Create;
using TaskFlow.Application.Features.Tasks.Get;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly CreateTaskHandler  _createTaskHandler;
    private readonly GetAllTasksHandler _getAllTasksHandler;

    public TasksController(CreateTaskHandler createTaskHandler, GetAllTasksHandler getAllTasksHandler)
    {
        _createTaskHandler  = createTaskHandler;
        _getAllTasksHandler  = getAllTasksHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { code = Error.Codes.Unauthorized, message = "Geçersiz token." });

        var result = await _createTaskHandler.HandleAsync(request, userId);

        return result.IsFailure
            ? ToErrorResponse(result.Error!)
            : CreatedAtAction(nameof(GetAll), new { }, result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { code = Error.Codes.Unauthorized, message = "Geçersiz token." });

        var result = await _getAllTasksHandler.HandleAsync(pageNumber, pageSize, userId);

        return result.IsFailure
            ? ToErrorResponse(result.Error!)
            : Ok(result.Value);
    }

    // Extracts the authenticated user's ID from the JWT NameIdentifier claim.
    // Returns false if the claim is missing or malformed — should never happen
    // for a valid token, but defensive programming is correct here.
    private bool TryGetUserId(out int userId)
    {
        userId = 0;
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim is not null && int.TryParse(claim, out userId);
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
