using Dapper;
using System.Data;
using TaskManager.Core.Common;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;
using TaskManager.Data.Constants;

namespace TaskManager.Data.Repositories
{
    public class TaskRepository(IDbConnection connection) : ITaskRepository
    {
        private readonly IDbConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        public async Task<Result<TaskItem>> Create(TaskItem taskItem)
        {
            var sql = @"
                INSERT INTO Tasks (Id, Title, Description, Status, Priority, CreatedAt, DueDate, CompletedAt, UpdatedAt)
                VALUES (@Id, @Title, @Description, @Status, @Priority, @CreatedAt, @DueDate, @CompletedAt, @UpdatedAt)";
            try
            {
                await _connection.ExecuteAsync(sql, taskItem);
                return Result<TaskItem>.Success(taskItem);
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error inserting task: {ex.Message}");
                var exceptionMessage = string.Format(RepositoryErrorMessages.DatabaseError, ex);
                return Result<TaskItem>.Failure(exceptionMessage);
            }
        }

        public async Task<Result<bool>> Delete(Guid id)
        {
            var sql = "UPDATE Tasks SET IsDeleted = 1 WHERE Id = @Id";

            try
            {
                var taskResult = await GetById(id);

                if (!taskResult.IsSuccess)
                {
                    // If GetById failed due to an exception, propagate the error message
                    var errors = taskResult.Errors.ToList() ?? [RepositoryErrorMessages.TaskNotFound];
                    return Result<bool>.Failure(errors);
                }

                if (taskResult.Value is null)
                {
                    return Result<bool>.Failure(RepositoryErrorMessages.TaskNotFound);
                }

                var affectedRows = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });
                return Result<bool>.Success(affectedRows > 0);
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error deleting task: {ex.Message}");
                var exceptionMessage = string.Format(RepositoryErrorMessages.DatabaseError, ex);
                return Result<bool>.Failure(exceptionMessage);
            }
        }

        public async Task<Result<IEnumerable<TaskItem>>> GetAll()
        {
            var sql = "SELECT * FROM Tasks WHERE IsDeleted = 0";

            try
            {
                var tasks = await _connection.QueryAsync<TaskItem>(sql);
                return Result<IEnumerable<TaskItem>>.Success(tasks);
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error retrieving tasks: {ex.Message}");
                var exceptionMessage = string.Format(RepositoryErrorMessages.DatabaseError, ex);
                return Result<IEnumerable<TaskItem>>.Failure(exceptionMessage);
            }
        }

        public async Task<Result<TaskItem>> GetById(Guid id)
        {
            var sql = "SELECT * FROM Tasks WHERE Id = @Id AND IsDeleted = 0";

            try
            {
                var task = await _connection.QuerySingleOrDefaultAsync<TaskItem>(sql, new { Id = id.ToString() });

                if (task == null)
                    return Result<TaskItem>.Failure(RepositoryErrorMessages.TaskNotFound);

                return Result<TaskItem>.Success(task);
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error retrieving task by ID: {ex.Message}");
                var exceptionMessage = string.Format(RepositoryErrorMessages.DatabaseError, ex);
                return Result<TaskItem>.Failure(exceptionMessage);
            }
        }

        public async Task<Result<IEnumerable<TaskItem>>> GetDeleted()
        {
            var sql = "SELECT * FROM Tasks WHERE IsDeleted = 1";

            try
            {
                var deletedTasks = await _connection.QueryAsync<TaskItem>(sql);
                return Result<IEnumerable<TaskItem>>.Success(deletedTasks);
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error retrieving deleted tasks: {ex.Message}");
                var exceptionMessage = string.Format(RepositoryErrorMessages.DatabaseError, ex);
                return Result<IEnumerable<TaskItem>>.Failure(exceptionMessage);
            }
        }

        public async Task<Result<bool>> Restore(Guid id)
        {
            var sql = "UPDATE Tasks SET IsDeleted = 0 WHERE Id = @Id";

            try
            {
                var affectedRows = await _connection.ExecuteAsync(sql, new { Id = id.ToString() });

                if (affectedRows == 0)
                    return Result<bool>.Failure(RepositoryErrorMessages.TaskNotFound);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error restoring task: {ex.Message}");
                var exceptionMessage = string.Format(RepositoryErrorMessages.DatabaseError, ex);
                return Result<bool>.Failure(exceptionMessage);
            }
        }

        public async Task<Result<TaskItem>> Update(TaskItem taskItem)
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

                if (affectedRows == 0)
                    return Result<TaskItem>.Failure(RepositoryErrorMessages.TaskNotFound);

                return Result<TaskItem>.Success(taskItem);
            }
            catch (Exception ex)
            {
                // Log the exception (Logger will be implemented later)
                Console.Error.WriteLine($"Error updating task: {ex.Message}");
                var exceptionMessage = string.Format(RepositoryErrorMessages.DatabaseError, ex);
                return Result<TaskItem>.Failure(exceptionMessage);
            }
        }
    }
}
