using TaskManager.Core.Common;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;
using TaskManager.Services.Interfaces;
using TaskManager.Services.Validators.Constants;

namespace TaskManager.Services
{
    public class TaskService(ITaskRepository repository, IValidator<TaskItem> validator) : ITaskService
    {
        private readonly ITaskRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IValidator<TaskItem> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        public async Task<Result<TaskItem?>> CreateAsync(TaskItem taskItem)
        {
            var validationResult = await _validator.ValidateAsync(taskItem);

            if (validationResult.IsValid)
            {
                var task = await _repository.Create(taskItem);
                return Result<TaskItem?>.Success(task.Value);
            }
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<TaskItem?>.Failure(errors);
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            var result = await _repository.Delete(id);
            return result.Value
                ? Result<bool>.Success(result.Value)
                : Result<bool>.Failure(new List<string> { ValidationMessages.TaskNotFound });
        }

        public async Task<Result<IEnumerable<TaskItem>>> GetAllAsync()
        {
            var tasks = await _repository.GetAll();
            return Result<IEnumerable<TaskItem>>.Success(tasks.Value ?? []);
        }

        public async Task<Result<TaskItem?>> GetByIdAsync(Guid id)
        {
            var task = await _repository.GetById(id);

            if (task.Value is null)
                return Result<TaskItem?>.Failure(new List<string> { ValidationMessages.TaskNotFound });

            return Result<TaskItem?>.Success(task.Value);
        }

        public async Task<Result<IEnumerable<TaskItem>>> GetDeletedAsync()
        {
            var deletedTasks = await _repository.GetDeleted();
            return Result<IEnumerable<TaskItem>>.Success(deletedTasks.Value ?? []);
        }

        public async Task<Result<bool>> RestoreAsync(Guid id)
        {
            var result = await _repository.Restore(id);
            return result.Value
                ? Result<bool>.Success(result.Value)
                : Result<bool>.Failure(new List<string> { ValidationMessages.TaskNotFound });
        }

        public async Task<Result<TaskItem?>> UpdateAsync(TaskItem taskItem)
        {
            var existingTask = await _repository.GetById(taskItem.Id);

            // It means two things:
            // 1. The task to be updated does not exist.
            // 2. The task has been soft-deleted and cannot be updated.
            if (existingTask.Value is null)
            {
                return Result<TaskItem?>.Failure(new List<string> { ValidationMessages.TaskCannotBeUpdated });
            }

            var validationResult = await _validator.ValidateAsync(taskItem);

            if (validationResult.IsValid)
            {
                var updatedTask = await _repository.Update(taskItem);
                return Result<TaskItem?>.Success(updatedTask.Value);
            }
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<TaskItem?>.Failure(errors);
        }
    }
}
