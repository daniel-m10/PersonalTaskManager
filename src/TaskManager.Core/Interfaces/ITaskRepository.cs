using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem> Create(TaskItem taskItem);
        Task<IEnumerable<TaskItem>> GetAll();
        Task<TaskItem?> GetById(Guid id);
        Task<TaskItem?> Update(TaskItem taskItem);
        Task<bool> Delete(Guid id);
    }
}
