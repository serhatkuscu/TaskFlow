using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Interfaces.Repositories;

namespace TaskFlow.Application.Features.Tasks.Get;

public class GetAllTasksHandler
{
    private readonly ITaskRepository _taskRepository;

    public GetAllTasksHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<List<TaskResponseDto>> HandleAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();

        return tasks.Select(task => new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            CreatedAt = task.CreatedAt
        }).ToList();
    }
}