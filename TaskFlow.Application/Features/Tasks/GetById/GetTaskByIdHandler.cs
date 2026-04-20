using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.Application.Features.Tasks.GetById;

public class GetTaskByIdHandler
{
    private readonly ITaskRepository              _taskRepository;
    private readonly ICurrentUserService          _currentUserService;
    private readonly ILogger<GetTaskByIdHandler>  _logger;

    public GetTaskByIdHandler(
        ITaskRepository             taskRepository,
        ICurrentUserService         currentUserService,
        ILogger<GetTaskByIdHandler> logger)
    {
        _taskRepository     = taskRepository;
        _currentUserService = currentUserService;
        _logger             = logger;
    }

    public async Task<Result<TaskResponseDto>> HandleAsync(Guid id)
    {
        var userId = _currentUserService.UserId;
        var task   = await _taskRepository.GetByIdAsync(id, userId);

        if (task is null)
        {
            _logger.LogWarning(
                "Task not found or access denied. TaskId: {TaskId}, UserId: {UserId}",
                id,
                userId);

            return Result<TaskResponseDto>.Failure(
                Error.Create(Error.Codes.NotFound, "Task not found."));
        }

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
