using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Create;
using TaskFlow.Application.Features.Tasks.Get;
using TaskFlow.Application.Features.Tasks.GetById;
using TaskFlow.Application.Features.Tasks.Delete;
using TaskFlow.Application.Features.Tasks.Update;
using TaskFlow.Application.Features.Tasks.UpdateStatus;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly CreateTaskHandler         _createTaskHandler;
    private readonly GetAllTasksHandler        _getAllTasksHandler;
    private readonly GetTaskByIdHandler        _getTaskByIdHandler;
    private readonly UpdateTaskHandler         _updateTaskHandler;
    private readonly DeleteTaskHandler         _deleteTaskHandler;
    private readonly UpdateTaskStatusHandler   _updateTaskStatusHandler;

    public TasksController(
        CreateTaskHandler        createTaskHandler,
        GetAllTasksHandler       getAllTasksHandler,
        GetTaskByIdHandler       getTaskByIdHandler,
        UpdateTaskHandler        updateTaskHandler,
        DeleteTaskHandler        deleteTaskHandler,
        UpdateTaskStatusHandler  updateTaskStatusHandler)
    {
        _createTaskHandler        = createTaskHandler;
        _getAllTasksHandler        = getAllTasksHandler;
        _getTaskByIdHandler       = getTaskByIdHandler;
        _updateTaskHandler        = updateTaskHandler;
        _deleteTaskHandler        = deleteTaskHandler;
        _updateTaskStatusHandler  = updateTaskStatusHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var result = await _createTaskHandler.HandleAsync(request);

        return result.IsFailure
            ? ToErrorResponse(result.Error!)
            : CreatedAtAction(nameof(GetAll), new { }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getTaskByIdHandler.HandleAsync(id);

        return result.IsFailure
            ? ToErrorResponse(result.Error!)
            : Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
    {
        var result = await _updateTaskHandler.HandleAsync(id, request);

        return result.IsFailure
            ? ToErrorResponse(result.Error!)
            : Ok(result.Value);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        var result = await _updateTaskStatusHandler.HandleAsync(id, request);

        return result.IsFailure
            ? ToErrorResponse(result.Error!)
            : Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _deleteTaskHandler.HandleAsync(id);

        return result.IsFailure
            ? ToErrorResponse(result.Error!)
            : NoContent();
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
