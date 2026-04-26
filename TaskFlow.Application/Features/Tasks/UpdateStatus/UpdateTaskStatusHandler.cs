using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Tasks.UpdateStatus;

public class UpdateTaskStatusHandler
{
    private readonly ITaskRepository                  _taskRepository;
    private readonly ICurrentUserService              _currentUserService;
    private readonly ILogger<UpdateTaskStatusHandler> _logger;

    public UpdateTaskStatusHandler(
        ITaskRepository                  taskRepository,
        ICurrentUserService              currentUserService,
        ILogger<UpdateTaskStatusHandler> logger)
    {
        _taskRepository     = taskRepository;
        _currentUserService = currentUserService;
        _logger             = logger;
    }

    public async Task<Result<TaskResponseDto>> HandleAsync(Guid id, UpdateTaskStatusRequest request)
    {
        var userId = _currentUserService.UserId;
        var task   = await _taskRepository.GetByIdAsync(id, userId);

        if (task is null)
        {
            _logger.LogWarning(
                "Status update failed: task not found or access denied. TaskId: {TaskId}, UserId: {UserId}",
                id,
                userId);

            return Result<TaskResponseDto>.Failure(
                Error.Create(Error.Codes.NotFound, "Task not found."));
        }

        // Validator guarantees the string is one of the three allowed values,
        // so TryParse will always succeed here. The explicit check is a safety net.
        if (!Enum.TryParse<TaskItemStatus>(request.Status, ignoreCase: true, out var targetStatus))
        {
            return Result<TaskResponseDto>.Failure(
                Error.Create(Error.Codes.Validation, $"Unknown status: {request.Status}."));
        }

        try
        {
            switch (targetStatus)
            {
                case TaskItemStatus.InProgress:
                    task.Start();
                    break;
                case TaskItemStatus.Completed:
                    task.Complete();
                    break;
                case TaskItemStatus.Cancelled:
                    task.Cancel();
                    break;
                default:
                    return Result<TaskResponseDto>.Failure(
                        Error.Create(Error.Codes.Validation, $"Status '{request.Status}' cannot be set directly."));
            }
        }
        catch (InvalidOperationException ex)
        {
            // Domain methods throw InvalidOperationException for illegal transitions
            // (e.g. completing a Pending task). Translate to a Validation failure
            // so callers get a consistent Result instead of an exception.
            _logger.LogWarning(
                "Invalid status transition. TaskId: {TaskId}, CurrentStatus: {CurrentStatus}, TargetStatus: {TargetStatus}, Reason: {Reason}",
                id,
                task.Status,
                targetStatus,
                ex.Message);

            return Result<TaskResponseDto>.Failure(
                Error.Create(Error.Codes.Validation, ex.Message));
        }

        await _taskRepository.UpdateAsync(task);

        _logger.LogInformation(
            "Task status updated. TaskId: {TaskId}, NewStatus: {Status}",
            id,
            task.Status);

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
