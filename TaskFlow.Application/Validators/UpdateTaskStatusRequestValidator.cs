using FluentValidation;
using TaskFlow.Application.DTOs.Task;

namespace TaskFlow.Application.Validators;

public class UpdateTaskStatusRequestValidator : AbstractValidator<UpdateTaskStatusRequest>
{
    // Pending is excluded: no domain method transitions back to Pending.
    private static readonly string[] AllowedStatuses = ["InProgress", "Completed", "Cancelled"];

    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status boş olamaz.")
            .Must(s => AllowedStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Geçerli status değerleri: {string.Join(", ", AllowedStatuses)}.");
    }
}
