using TaskFlow.Application.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces.Repositories;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task);
    Task<(List<TaskItem> Items, int TotalCount)> GetAllAsync(PaginationQuery query);
}
