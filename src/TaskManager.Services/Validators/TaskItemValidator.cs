using FluentValidation;
using FluentValidation.Results;
using TaskManager.Core.Entities;
using TaskManager.Services.Validators.Constants;

namespace TaskManager.Services.Validators
{
    public class TaskItemValidator : AbstractValidator<TaskItem>, TaskManager.Services.Interfaces.IValidator<TaskItem>
    {
        public TaskItemValidator()
        {
            // Required fields
            RuleFor(task => task.Title)
                .NotEmpty().WithMessage(ValidationMessages.TitleRequired)
                .MaximumLength(100).WithMessage(ValidationMessages.TitleLength);
            RuleFor(task => task.Status)
                .IsInEnum().WithMessage(ValidationMessages.InvalidStatus);
            RuleFor(task => task.Priority)
                .IsInEnum().WithMessage(ValidationMessages.InvalidPriority);
            RuleFor(task => task.CreatedAt)
                .NotEqual(default(DateTime)).WithMessage(ValidationMessages.CreatedAtRequired);

            // Optional fields
            RuleFor(task => task.Description)
                .MaximumLength(500).WithMessage(ValidationMessages.DescriptionLength);
            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Today)
                .When(x => x.DueDate.HasValue)
                .WithMessage(ValidationMessages.DueDateNotInPast);
            RuleFor(task => task.CompletedAt)
                .GreaterThan(task => task.CreatedAt).WithMessage(ValidationMessages.CompletedAtAfterCreatedAt)
                .When(task => task.CompletedAt.HasValue);
            RuleFor(task => task.UpdatedAt)
                .GreaterThan(task => task.CreatedAt).WithMessage(ValidationMessages.UpdatedAtAfterCreatedAt)
                .When(task => task.UpdatedAt.HasValue);
        }

        public async Task<ValidationResult> ValidateAsync(TaskItem task)
        {
            return await base.ValidateAsync(task);
        }
    }
}
