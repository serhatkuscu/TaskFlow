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
        var result = await _createTaskHandler.HandleAsync(request);

        return result.IsFailure
            ? ToErrorResponse(result.Error!)
            : CreatedAtAction(nameof(GetAll), new { }, result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _getAllTasksHandler.HandleAsync(pageNumber, pageSize);

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
