using Dapper;
using System.Data;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Data.Repositories
{
    public class TaskRepository(IDbConnection connection) : ITaskRepository
    {
        private readonly IDbConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        public async Task<TaskItem> Create(TaskItem taskItem)
        {
            var sql = @"
                INSERT INTO Tasks (Id, Title, Description, Status, Priority, CreatedAt, DueDate, CompletedAt, UpdatedAt)
                VALUES (@Id, @Title, @Description, @Status, @Priority, @CreatedAt, @DueDate, @CompletedAt, @UpdatedAt)";
            try
            {
                await _connection.ExecuteAsync(sql, taskItem);
                return taskItem;
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error inserting task: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }
        }

        public async Task<bool> Delete(Guid id)
        {
            var sql = "DELETE FROM Tasks WHERE Id = @Id";

            try
            {
                var affectedRows = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error deleting task: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }
        }

        public async Task<IEnumerable<TaskItem>> GetAll()
        {
            var sql = "SELECT * FROM Tasks";

            try
            {
                var tasks = await _connection.QueryAsync<TaskItem>(sql);
                return tasks;
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error retrieving tasks: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }
        }

        public async Task<TaskItem?> GetById(Guid id)
        {
            var sql = "SELECT * FROM Tasks WHERE Id = @Id";

            try
            {
                var task = await _connection.QuerySingleOrDefaultAsync<TaskItem>(sql, new { Id = id.ToString() });
                return task;
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error retrieving task by ID: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }
        }

        public async Task<TaskItem?> Update(TaskItem taskItem)
        {
            var sql = @"
                UPDATE Tasks
                SET Title = @Title,
                    Description = @Description,
                    Status = @Status,
                    Priority = @Priority,
                    DueDate = @DueDate,
                    CompletedAt = @CompletedAt,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var parameters = new
            {
                Id = taskItem.Id.ToString(), // Ensure string type for SQLite TEXT
                taskItem.Title,
                taskItem.Description,
                Status = (int)taskItem.Status,
                Priority = (int)taskItem.Priority,
                DueDate = taskItem.DueDate?.ToString("o"),
                CompletedAt = taskItem.CompletedAt?.ToString("o"),
                UpdatedAt = taskItem.UpdatedAt?.ToString("o")
            };

            try
            {
                var affectedRows = await _connection.ExecuteAsync(sql, parameters);
                return affectedRows > 0 ? taskItem : null;
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error updating task: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }
        }
    }
}
