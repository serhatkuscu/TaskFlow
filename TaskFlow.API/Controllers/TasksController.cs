using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Create;
using TaskFlow.Application.Features.Tasks.Get;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly CreateTaskHandler _createTaskHandler;
    private readonly GetAllTasksHandler _getAllTasksHandler;

    public TasksController(
        CreateTaskHandler createTaskHandler,
        GetAllTasksHandler getAllTasksHandler)
    {
        _createTaskHandler = createTaskHandler;
        _getAllTasksHandler = getAllTasksHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        await _createTaskHandler.HandleAsync(request);
        return Ok(new { message = "Task created successfully." });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _getAllTasksHandler.HandleAsync();
        return Ok(tasks);
    }
}