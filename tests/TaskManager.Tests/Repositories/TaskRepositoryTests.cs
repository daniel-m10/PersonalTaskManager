using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
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
                    UpdatedAt TEXT
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
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<TaskItem>());
                Assert.That(result.Id, Is.Not.Default);
                Assert.That(result.Title, Is.EqualTo("Test Task"));
                Assert.That(result.Status, Is.EqualTo(Status.InProgress));
                Assert.That(result.Priority, Is.EqualTo(Priority.Medium));
                Assert.That(result.CreatedAt, Is.Not.Default);
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
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(guid));
                Assert.That(result.Title, Is.EqualTo(taskItem.Title));
                Assert.That(result.Status, Is.EqualTo(taskItem.Status));
                Assert.That(result.Priority, Is.EqualTo(taskItem.Priority));
                Assert.That(result.CreatedAt, Is.EqualTo(taskItem.CreatedAt));
                Assert.That(result.Description, Is.EqualTo(taskItem.Description));
                Assert.That(result.DueDate, Is.EqualTo(taskItem.DueDate));
                Assert.That(result.CompletedAt, Is.EqualTo(taskItem.CompletedAt));
                Assert.That(result.UpdatedAt, Is.EqualTo(taskItem.UpdatedAt));
            }
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
            Assert.That(result.Id, Is.EqualTo(task.Id));
        }

        [Test]
        public async Task GetById_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var result = await _repository.GetById(id);

            // Assert
            Assert.That(result, Is.Null);
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
            Assert.That(result, Is.InstanceOf<IEnumerable<TaskItem>>());
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAll_ShouldReturnEmptyCollection_WhenNoTasksExist()
        {
            // Arrange & Act
            var result = await _repository.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<IEnumerable<TaskItem>>());
            Assert.That(result, Is.Empty);
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
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Title, Is.EqualTo(updatedTitle));
                Assert.That(result.UpdatedAt, Is.Not.Null);
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

            // Act & Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task Delete_ShouldReturnTrue_WhenTaskIsDeleted()
        {
            // Arrange
            var task = InsertTaskItem();

            // Act
            var result = await _repository.Delete(task.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task Delete_ShouldReturnFalse_WhenTaskDoesNotExist()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var result = await _repository.Delete(guid);

            // Assert
            Assert.That(result, Is.False);
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

        private IEnumerable<TaskItem> InsertMultipleTaskItems(int count)
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
