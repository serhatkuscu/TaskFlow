using FluentValidation;
using TaskFlow.Application.DTOs.Task;

namespace TaskFlow.Application.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title boş olamaz.")
            .MinimumLength(3).WithMessage("Title en az 3 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Title en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description en fazla 1000 karakter olabilir.");
    }
}