using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskFlowDbContext _context;

    public TaskRepository(TaskFlowDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TaskItem task)
    {
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        // EF Core tracks the entity fetched by GetByIdAsync (no AsNoTracking),
        // so calling SaveChangesAsync is enough to persist the changes.
        await _context.SaveChangesAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, int userId)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
    }

    public async Task<(List<TaskItem> Items, int TotalCount)> GetAllAsync(PaginationQuery query, int userId)
    {
        // AsNoTracking: read-only query — no need to track entities in the change tracker.
        // Filter by userId so each user only sees their own tasks.
        var baseQuery = _context.Tasks
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await baseQuery.CountAsync();

        var items = await baseQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}