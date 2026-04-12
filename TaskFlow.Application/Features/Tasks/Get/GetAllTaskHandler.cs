using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.Application.Features.Tasks.Get;

public class GetAllTasksHandler
{
    private readonly ITaskRepository             _taskRepository;
    private readonly ICurrentUserService         _currentUserService;
    private readonly ILogger<GetAllTasksHandler> _logger;

    public GetAllTasksHandler(
        ITaskRepository            taskRepository,
        ICurrentUserService        currentUserService,
        ILogger<GetAllTasksHandler> logger)
    {
        _taskRepository     = taskRepository;
        _currentUserService = currentUserService;
        _logger             = logger;
    }

    public async Task<Result<PagedResult<TaskResponseDto>>> HandleAsync(int pageNumber, int pageSize)
    {
        var queryResult = PaginationQuery.Create(pageNumber, pageSize);
        if (queryResult.IsFailure)
        {
            _logger.LogWarning(
                "Invalid pagination parameters. PageNumber: {PageNumber}, PageSize: {PageSize}, Error: {Error}",
                pageNumber,
                pageSize,
                queryResult.Error!.Message);
            return Result<PagedResult<TaskResponseDto>>.Failure(queryResult.Error!);
        }

        var query  = queryResult.Value;
        var userId = _currentUserService.UserId;

        var (items, totalCount) = await _taskRepository.GetAllAsync(query, userId);

        var dtos = items.Select(task => new TaskResponseDto
        {
            Id          = task.Id,
            Title       = task.Title,
            Description = task.Description,
            Status      = task.Status.ToString(),
            CreatedAt   = task.CreatedAt
        }).ToList();

        return Result<PagedResult<TaskResponseDto>>.Success(
            new PagedResult<TaskResponseDto>(dtos, totalCount, query.PageNumber, query.PageSize));
    }
}
