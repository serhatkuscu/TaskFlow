using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.Application.Features.Tasks.Delete;

public class DeleteTaskHandler
{
    private readonly ITaskRepository            _taskRepository;
    private readonly ICurrentUserService        _currentUserService;
    private readonly ILogger<DeleteTaskHandler> _logger;

    public DeleteTaskHandler(
        ITaskRepository            taskRepository,
        ICurrentUserService        currentUserService,
        ILogger<DeleteTaskHandler> logger)
    {
        _taskRepository     = taskRepository;
        _currentUserService = currentUserService;
        _logger             = logger;
    }

    public async Task<Result> HandleAsync(Guid id)
    {
        var userId = _currentUserService.UserId;
        var task   = await _taskRepository.GetByIdAsync(id, userId);

        if (task is null)
        {
            _logger.LogWarning(
                "Delete failed: task not found or access denied. TaskId: {TaskId}, UserId: {UserId}",
                id,
                userId);

            return Result.Failure(
                Error.Create(Error.Codes.NotFound, "Task not found."));
        }

        await _taskRepository.DeleteAsync(task);

        _logger.LogInformation(
            "Task deleted. TaskId: {TaskId}, UserId: {UserId}",
            id,
            userId);

        return Result.Success();
    }
}
