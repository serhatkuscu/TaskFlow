using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Create;

public class CreateTaskHandler
{
    private readonly ITaskRepository          _taskRepository;
    private readonly ILogger<CreateTaskHandler> _logger;

    public CreateTaskHandler(ITaskRepository taskRepository, ILogger<CreateTaskHandler> logger)
    {
        _taskRepository = taskRepository;
        _logger         = logger;
    }

    // userId comes from the JWT token, extracted by the controller.
    // The handler never touches HttpContext — it receives plain data only.
    public async Task<Result<TaskResponseDto>> HandleAsync(CreateTaskRequest request, int userId)
    {
        var task = new TaskItem(request.Title, request.Description, userId);

        await _taskRepository.AddAsync(task);

        _logger.LogInformation(
            "Task created. Id: {TaskId}, Title: {Title}",
            task.Id,
            task.Title);

        var response = new TaskResponseDto
        {
            Id          = task.Id,
            Title       = task.Title,
            Description = task.Description,
            Status      = task.Status.ToString(),
            CreatedAt   = task.CreatedAt
        };

        return Result<TaskResponseDto>.Success(response);
    }
}
