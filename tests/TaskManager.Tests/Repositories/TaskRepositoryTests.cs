using Dapper;
using Microsoft.Data.Sqlite;
using NSubstitute;
using System.Data;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Data.Constants;
using TaskManager.Data.Handlers;
using TaskManager.Data.Repositories;

namespace TaskManager.Tests.Repositories
{
    [TestFixture]
    public class TaskRepositoryTests
    {
        private TaskRepository _repository;
        private IDbConnection _connection;

        [SetUp]
        public void SetUp()
        {
            // Register custom type handler for Guid
            SqlMapper.AddTypeHandler(new GuidTypeHandler());

            // Create in-memory Sqlite database and Tasks table
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            // Crete table schema for Tasks
            var createTableCmd = _connection.CreateCommand();

            createTableCmd.CommandText = @"
                CREATE TABLE Tasks (
                    Id TEXT PRIMARY KEY,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    Status INTEGER NOT NULL,
                    Priority INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    DueDate TEXT,
                    CompletedAt TEXT,
                    UpdatedAt TEXT,
                    IsDeleted INTEGER NOT NULL DEFAULT 0
                )";

            createTableCmd.ExecuteNonQuery();

            _repository = new TaskRepository(_connection);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Close();
            _connection.Dispose();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenConnectionIsNull()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new TaskRepository(null!));
            Assert.That(ex.ParamName, Is.EqualTo("connection"));
        }

        [Test]
        public async Task Create_ShouldReturnTaskItem_WhenValidTaskIsProvided()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Test Task",
                Status = Status.InProgress,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.Create(taskItem);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<TaskItem>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Value, Is.InstanceOf<TaskItem>());
                Assert.That(result.Value.Id, Is.Not.Default);
                Assert.That(result.Value.Title, Is.EqualTo("Test Task"));
                Assert.That(result.Value.Status, Is.EqualTo(Status.InProgress));
                Assert.That(result.Value.Priority, Is.EqualTo(Priority.Medium));
                Assert.That(result.Value.CreatedAt, Is.Not.Default);
            }
        }

        [Test]
        public async Task Create_ShouldReturnTaskItemWithSameId_WhenTaskIsProvided()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var taskItem = new TaskItem
            {
                Id = guid,
                Title = "Test Task",
                Status = Status.InProgress,
                Priority = Priority.Medium,
                CreatedAt = now,
                Description = "Test Description",
                DueDate = now.AddDays(2),
                CompletedAt = null,
                UpdatedAt = null
            };

            // Act
            var result = await _repository.Create(taskItem);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<TaskItem>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Errors, Is.Empty);
                Assert.That(result.Value.Id, Is.EqualTo(guid));
                Assert.That(result.Value.Title, Is.EqualTo(taskItem.Title));
                Assert.That(result.Value.Status, Is.EqualTo(taskItem.Status));
                Assert.That(result.Value.Priority, Is.EqualTo(taskItem.Priority));
                Assert.That(result.Value.CreatedAt, Is.EqualTo(taskItem.CreatedAt));
                Assert.That(result.Value.Description, Is.EqualTo(taskItem.Description));
                Assert.That(result.Value.DueDate, Is.EqualTo(taskItem.DueDate));
                Assert.That(result.Value.CompletedAt, Is.EqualTo(taskItem.CompletedAt));
                Assert.That(result.Value.UpdatedAt, Is.EqualTo(taskItem.UpdatedAt));
            }
        }

        [Test]
        public async Task Create_ShouldReturnFailure_WhenDatabaseThrowsException()
        {
            // Arrange
            var connection = Substitute.For<IDbConnection>();
            connection.When(x => x.Open()).Do(x => { throw new Exception("DB connection error"); });
            var repository = new TaskRepository(connection);

            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Test Task",
                Status = Status.InProgress,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await repository.Create(taskItem);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors.Any(e => e.Contains("Database error")), Is.True);
            }
        }

        [Test]
        public async Task Create_ShouldStoreTaskSafely_WhenTitleContainsSqlInjectionPattern()
        {
            // Arrange
            var maliciousTitle = "Test Task; DROP TABLE Tasks; --";
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = maliciousTitle,
                Status = Status.InProgress,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.Create(taskItem);

            // Assert
            Assert.That(result.Value, Is.InstanceOf<TaskItem>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Errors, Is.Empty);
                Assert.That(result.Value.Title, Is.EqualTo(maliciousTitle));
            }

            // Verify that the Tasks table still exists by fetching all tasks
            var allTasks = await _repository.GetAll();
            Assert.That(allTasks.Value, Is.InstanceOf<IEnumerable<TaskItem>>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(allTasks.IsSuccess, Is.True);
                Assert.That(allTasks.Value.Any(t => t.Title == maliciousTitle), Is.True);
            }
        }

        [Test]
        public async Task Create_ShouldHandleConcurrentInsertsCorrectly()
        {
            // Arrange
            var tasksToInsert = Enumerable.Range(0, 10)
                .Select(i => new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = $"Concurrent Task {i}",
                    Status = Status.Todo,
                    Priority = Priority.Medium,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

            // Act
            var insertTasks = tasksToInsert.Select(_repository.Create);
            var results = await Task.WhenAll(insertTasks);

            // Assert
            Assert.That(results.All(r => r.IsSuccess));

            var allTasks = await _repository.GetAll();
            Assert.That(allTasks.Value, Is.InstanceOf<IEnumerable<TaskItem>>());
            Assert.That(allTasks.Value.Count(), Is.GreaterThanOrEqualTo(tasksToInsert.Count));
            foreach (var task in tasksToInsert)
                Assert.That(allTasks.Value.Any(t => t.Id == task.Id));
        }

        [Test]
        public async Task GetById_ShouldReturnTaskItem_WhenTaskExists()
        {
            // Arrange
            var task = InsertTaskItem();

            // Act
            var result = await _repository.GetById(task.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<TaskItem>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Id, Is.EqualTo(task.Id));
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task GetById_ShouldReturnNull_WhenTaskIsNotFound()
        {
            // Arrange
            var task = InsertTaskItem();
            await _repository.Delete(task.Id);

            // Act
            var result = await _repository.GetById(task.Id);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Does.Contain(RepositoryErrorMessages.TaskNotFound));
            }
        }

        [Test]
        public async Task GetById_ShouldReturnFailure_WhenDatabaseThrowsException()
        {
            // Arrange
            var connection = Substitute.For<IDbConnection>();
            connection.When(x => x.Open()).Do(x => { throw new Exception("DB connection error"); });
            var repository = new TaskRepository(connection);
            var guid = Guid.NewGuid();

            // Act
            var result = await repository.GetById(guid);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors.Any(e => e.Contains("Database error")), Is.True);
            }
        }

        [Test]
        public async Task GetById_ShouldReturnFailure_WhenGuidIsEmpty()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act
            var result = await _repository.GetById(emptyGuid);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Does.Contain(RepositoryErrorMessages.TaskNotFound));
            }
        }

        [Test]
        public async Task GetAll_ShouldReturnAllTaskItems_WhenTasksExist()
        {
            // Arrange
            InsertMultipleTaskItems(2);

            // Act
            var result = await _repository.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<IEnumerable<TaskItem>>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Errors, Is.Empty);
                Assert.That(result.Value.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public async Task GetAll_ShouldNotReturnDeletedTasks_WhenCalled()
        {
            // Arrange
            var tasks = InsertMultipleTaskItems(3);
            // Delete one task
            await _repository.Delete(tasks[0].Id);

            // Act
            var result = await _repository.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<IEnumerable<TaskItem>>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Count(), Is.EqualTo(2));
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task GetAll_ShouldReturnEmptyCollection_WhenNoTasksExist()
        {
            // Arrange & Act
            var result = await _repository.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<IEnumerable<TaskItem>>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.Empty);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task GetAll_ShouldReturnOnlyActiveTasks_WhenCalled()
        {
            // Arrange
            var tasks = InsertMultipleTaskItems(5);
            // Delete two tasks
            await _repository.Delete(tasks[1].Id);
            await _repository.Delete(tasks[3].Id);

            // Act
            var result = await _repository.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<IEnumerable<TaskItem>>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Count(), Is.EqualTo(3));
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task GetAll_ShouldReturnFailure_WhenDatabaseThrowsException()
        {
            // Arrange
            var connection = Substitute.For<IDbConnection>();
            connection.When(x => x.Open()).Do(x => { throw new Exception("DB connection error"); });
            var repository = new TaskRepository(connection);

            // Act
            var result = await repository.GetAll();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors.Any(e => e.Contains("Database error")), Is.True);
            }
        }

        [Test]
        public async Task Update_ShouldReturnUpdatedTaskItem_WhenTaskExists()
        {
            // Arrange
            var guid = InsertTaskItem().Id;
            var updatedTitle = "Updated Task Title";

            var taskItem = new TaskItem
            {
                Id = guid,
                Title = updatedTitle,
                Status = Status.InProgress,
                Priority = Priority.High,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.Update(taskItem);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<TaskItem>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Title, Is.EqualTo(updatedTitle));
                Assert.That(result.Value.UpdatedAt, Is.Not.Null);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task Update_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Non-existent Task",
                Status = Status.InProgress,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.Update(taskItem);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Does.Contain(RepositoryErrorMessages.TaskNotFound));
            }
        }

        [Test]
        public async Task Update_ShouldReturnFailure_WhenDatabaseThrowsException()
        {
            // Arrange
            var connection = Substitute.For<IDbConnection>();
            connection.When(x => x.Open()).Do(x => { throw new Exception("DB connection error"); });
            var repository = new TaskRepository(connection);
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Test Task",
                Status = Status.InProgress,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await repository.Update(taskItem);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors.Any(e => e.Contains("Database error")), Is.True);
            }
        }

        [Test]
        public async Task Update_ShouldReturnFailure_WhenGuidIsEmpty()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.Empty,
                Title = "Test Task",
                Status = Status.InProgress,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.Update(taskItem);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Does.Contain(RepositoryErrorMessages.TaskNotFound));
            }
        }

        [Test]
        public async Task Update_ShouldStoreTaskSafely_WhenTitleContainsSqlInjectionPattern()
        {
            // Arrange
            var task = InsertTaskItem();
            var maliciousTitle = "Updated Task; DROP TABLE Tasks; --";

            var taskItem = new TaskItem
            {
                Id = task.Id,
                Title = maliciousTitle,
                Status = Status.InProgress,
                Priority = Priority.Medium,
                CreatedAt = task.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.Update(taskItem);

            // Assert
            Assert.That(result.Value, Is.InstanceOf<TaskItem>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Errors, Is.Empty);
                Assert.That(result.Value.Title, Is.EqualTo(maliciousTitle));
            }

            // Verify that the Tasks table still exists by fetching all tasks
            var allTasks = await _repository.GetAll();
            Assert.That(allTasks.Value, Is.InstanceOf<IEnumerable<TaskItem>>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(allTasks.IsSuccess, Is.True);
                Assert.That(allTasks.Value.Any(t => t.Title == maliciousTitle), Is.True);
            }
        }

        [Test]
        public async Task Delete_ShouldReturnTrue_WhenTaskIsDeleted()
        {
            // Arrange
            var task = InsertTaskItem();

            // Act
            var result = await _repository.Delete(task.Id);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.True);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task Delete_ShouldReturnFalse_WhenTaskDoesNotExist()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var result = await _repository.Delete(guid);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.False);
                Assert.That(result.Errors, Does.Contain(RepositoryErrorMessages.TaskNotFound));
            }
        }

        [Test]
        public async Task Delete_ShouldSetIsDeletedToTrue_WhenTaskIsDeleted()
        {
            // Arrange
            var task = InsertTaskItem();
            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = "SELECT IsDeleted FROM Tasks WHERE Id = @Id";
            selectCmd.Parameters.Add(new SqliteParameter("@Id", task.Id.ToString()));
            var isDeletedBefore = Convert.ToInt32(selectCmd.ExecuteScalar());
            Assert.That(isDeletedBefore, Is.Zero); // Ensure IsDeleted is false before deletion

            // Act
            var result = await _repository.Delete(task.Id);
            var isDeletedAfter = Convert.ToInt32(selectCmd.ExecuteScalar());

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.True);
                Assert.That(result.Errors, Is.Empty);
                Assert.That(isDeletedAfter, Is.EqualTo(1)); // Ensure IsDeleted is true after deletion
            }
        }

        [Test]
        public async Task Delete_ShouldReturnFailure_WhenDatabaseThrowsException()
        {
            // Arrange
            var connection = Substitute.For<IDbConnection>();
            connection.When(x => x.Open()).Do(x => { throw new Exception("DB connection error"); });
            var repository = new TaskRepository(connection);
            var guid = Guid.NewGuid();

            // Act
            var result = await repository.Delete(guid);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.False);
                Assert.That(result.Errors.Any(e => e.Contains("Database error")), Is.True);
            }
        }

        [Test]
        public async Task Delete_ShouldReturnFailure_WhenGuidIsEmpty()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act
            var result = await _repository.Delete(emptyGuid);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.False);
                Assert.That(result.Errors, Does.Contain(RepositoryErrorMessages.TaskNotFound));
            }
        }

        [Test]
        public async Task GetDeleted_ShouldReturnOnlyDeletedTasks_WhenCalled()
        {
            // Arrange
            var tasks = InsertMultipleTaskItems(4);
            // Delete two tasks
            await _repository.Delete(tasks[0].Id);
            await _repository.Delete(tasks[2].Id);

            // Act
            var result = await _repository.GetDeleted();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<IEnumerable<TaskItem>>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Count(), Is.EqualTo(2));
                Assert.That(result.Value.Select(t => t.IsDeleted), Is.All.True);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task GetDeleted_ShouldReturnFailure_WhenDatabaseThrowsException()
        {
            // Arrange
            var connection = Substitute.For<IDbConnection>();
            connection.When(x => x.Open()).Do(x => { throw new Exception("DB connection error"); });
            var repository = new TaskRepository(connection);

            // Act
            var result = await repository.GetDeleted();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors.Any(e => e.Contains("Database error")), Is.True);
            }
        }

        [Test]
        public async Task Restore_ShouldSetIsDeletedToFalse_WhenTaskIsRestored()
        {
            // Arrange
            var task = InsertTaskItem();
            await _repository.Delete(task.Id);

            // Act
            var result = await _repository.Restore(task.Id);

            // Assert
            var currentTask = await _repository.GetById(task.Id);

            Assert.That(currentTask.Value, Is.InstanceOf<TaskItem?>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.True);
                Assert.That(result.Errors, Is.Empty);
                Assert.That(currentTask, Is.Not.Null);
                Assert.That(currentTask.Value.IsDeleted, Is.False);
            }
        }

        [Test]
        public async Task Restore_ShouldReturnFailure_WhenDatabaseThrowsException()
        {
            // Arrange
            var connection = Substitute.For<IDbConnection>();
            connection.When(x => x.Open()).Do(x => { throw new Exception("DB connection error"); });
            var repository = new TaskRepository(connection);
            var guid = Guid.NewGuid();

            // Act
            var result = await repository.Restore(guid);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.False);
                Assert.That(result.Errors.Any(e => e.Contains("Database error")), Is.True);
            }
        }

        [Test]
        public async Task Restore_ShouldReturnFailure_WhenGuidIsEmpty()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act
            var result = await _repository.Restore(emptyGuid);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.False);
                Assert.That(result.Errors, Does.Contain(RepositoryErrorMessages.TaskNotFound));
            }
        }

        private TaskItem InsertTaskItem()
        {
            var guid = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var taskItem = new TaskItem
            {
                Id = guid,
                Title = "Test Task",
                Status = Status.InProgress,
                Priority = Priority.Medium,
                CreatedAt = now,
                Description = "Test Description",
                DueDate = now.AddDays(2),
                CompletedAt = null,
                UpdatedAt = null
            };

            var createCmd = _connection.CreateCommand();
            createCmd.CommandText = @"
                INSERT INTO Tasks (Id, Title, Description, Status, Priority, CreatedAt, DueDate, CompletedAt, UpdatedAt)
                VALUES (@Id, @Title, @Description, @Status, @Priority, @CreatedAt, @DueDate, @CompletedAt, @UpdatedAt)";

            createCmd.Parameters.Add(new SqliteParameter("@Id", taskItem.Id.ToString()));
            createCmd.Parameters.Add(new SqliteParameter("@Title", taskItem.Title));
            createCmd.Parameters.Add(new SqliteParameter("@Description", taskItem.Description));
            createCmd.Parameters.Add(new SqliteParameter("@Status", (int)taskItem.Status));
            createCmd.Parameters.Add(new SqliteParameter("@Priority", (int)taskItem.Priority));
            createCmd.Parameters.Add(new SqliteParameter("@CreatedAt", taskItem.CreatedAt.ToString("o")));
            createCmd.Parameters.Add(new SqliteParameter("@DueDate", taskItem.DueDate?.ToString("o") ?? (object)DBNull.Value));
            createCmd.Parameters.Add(new SqliteParameter("@CompletedAt", taskItem.CompletedAt?.ToString("o") ?? (object)DBNull.Value));
            createCmd.Parameters.Add(new SqliteParameter("@UpdatedAt", taskItem.UpdatedAt?.ToString("o") ?? (object)DBNull.Value));
            createCmd.ExecuteNonQuery();

            return taskItem;
        }

        private List<TaskItem> InsertMultipleTaskItems(int count)
        {
            var tasks = new List<TaskItem>();
            for (int i = 0; i < count; i++)
            {
                tasks.Add(InsertTaskItem());
            }
            return tasks;
        }
    }
}
