using TaskFlow.Application.Common;
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

    public async Task<Result<PagedResult<TaskResponseDto>>> HandleAsync(int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1)
            return Result<PagedResult<TaskResponseDto>>.Failure(
                Error.Create(Error.Codes.Validation, "Sayfa numarası 1'den küçük olamaz."));

        if (pageSize < 1 || pageSize > 50)
            return Result<PagedResult<TaskResponseDto>>.Failure(
                Error.Create(Error.Codes.Validation, "Sayfa boyutu 1 ile 50 arasında olmalıdır."));

        var (items, totalCount) = await _taskRepository.GetAllAsync(pageNumber, pageSize);

        var dtos = items.Select(task => new TaskResponseDto
        {
            Id          = task.Id,
            Title       = task.Title,
            Description = task.Description,
            Status      = task.Status.ToString(),
            CreatedAt   = task.CreatedAt
        }).ToList();

        var pagedResult = new PagedResult<TaskResponseDto>(dtos, totalCount, pageNumber, pageSize);

        return Result<PagedResult<TaskResponseDto>>.Success(pagedResult);
    }
}
