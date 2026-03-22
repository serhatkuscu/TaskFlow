using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Get;

public class GetAllTasksHandler
{
    private readonly ITaskRepository _taskRepository;

    public GetAllTasksHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<List<TaskItem>> HandleAsync()
    {
        return await _taskRepository.GetAllAsync();
    }
}