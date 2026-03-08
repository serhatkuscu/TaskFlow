namespace TaskFlow.Application.DTOs.Task;

public class CreateTaskRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}
