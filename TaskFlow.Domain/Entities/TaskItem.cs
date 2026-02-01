using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }

    private TaskItem() { } // EF Core için

    public TaskItem(string title, string? description)
    {
        Title = title;
        Description = description;
        Status = TaskItemStatus.Pending;
    }

    public void Start()
    {
        if (Status != TaskItemStatus.Pending)
            throw new InvalidOperationException("Task already started.");

        Status = TaskItemStatus.InProgress;
    }

    public void Complete()
    {
        if (Status != TaskItemStatus.InProgress)
            throw new InvalidOperationException("Only in-progress tasks can be completed.");

        Status = TaskItemStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == TaskItemStatus.Completed)
            throw new InvalidOperationException("Completed task cannot be cancelled.");

        Status = TaskItemStatus.Cancelled;
    }
}
