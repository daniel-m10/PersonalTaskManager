using TaskManager.Core.Common;
using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<Result<TaskItem>> Create(TaskItem taskItem);
        Task<Result<IEnumerable<TaskItem>>> GetAll();
        Task<Result<TaskItem>> GetById(Guid id);
        Task<Result<TaskItem>> Update(TaskItem taskItem);
        Task<Result<bool>> Delete(Guid id);
        Task<Result<IEnumerable<TaskItem>>> GetDeleted();
        Task<Result<bool>> Restore(Guid id);
    }
}
