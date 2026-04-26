using TaskFlow.Application.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces.Repositories;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task);
    Task<(List<TaskItem> Items, int TotalCount)> GetAllAsync(PaginationQuery query, int userId);
    Task<TaskItem?> GetByIdAsync(Guid id, int userId);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
}
