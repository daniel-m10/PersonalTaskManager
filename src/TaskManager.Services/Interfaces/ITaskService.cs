using TaskManager.Core.Common;
using TaskManager.Core.Entities;

namespace TaskManager.Services.Interfaces
{
    public interface ITaskService
    {
        Task<Result<TaskItem?>> CreateAsync(TaskItem taskItem);
        Task<Result<IEnumerable<TaskItem>>> GetAllAsync();
        Task<Result<TaskItem?>> GetByIdAsync(Guid id);
        Task<Result<TaskItem?>> UpdateAsync(TaskItem taskItem);
        Task<Result<bool>> DeleteAsync(Guid id); // Soft delete
        Task<Result<IEnumerable<TaskItem>>> GetDeletedAsync(); // Get soft-deleted tasks
        Task<Result<bool>> RestoreAsync(Guid id); // Restore soft-deleted task
    }
}
