using FluentValidation.Results;

namespace TaskManager.Services.Interfaces
{
    public interface IValidator<T>
    {
        Task<ValidationResult> ValidateAsync(T entity);
    }
}
