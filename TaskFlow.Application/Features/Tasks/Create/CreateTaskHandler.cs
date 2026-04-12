using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Create;

public class CreateTaskHandler
{
    private readonly ITaskRepository _taskRepository;

    public CreateTaskHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskResponseDto>> HandleAsync(CreateTaskRequest request)
    {
        var task = new TaskItem(request.Title, request.Description);

        await _taskRepository.AddAsync(task);

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
