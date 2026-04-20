namespace TaskFlow.Application.DTOs.Task;

public class UpdateTaskRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}
