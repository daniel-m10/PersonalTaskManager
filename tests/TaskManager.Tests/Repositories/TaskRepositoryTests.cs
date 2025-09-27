using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Core.Interfaces;

namespace TaskManager.Tests.Repositories
{
    [TestFixture]
    public class TaskRepositoryTests
    {
        private ITaskRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _repository = Substitute.For<ITaskRepository>();
        }

        // TODO: Update tests when repository is implemented, use in-memory Sqlite

        [Test]
        public async Task Create_ShouldReturnTaskItem_WhenValidTaskIsProvided()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Test Task",
                Status = Status.Pending,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow
            };

            _repository.Create(Arg.Any<TaskItem>()).Returns(taskItem);

            // Act
            var result = await _repository.Create(taskItem);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<TaskItem>());
                Assert.Multiple(() =>
                {
                    Assert.That(result.Id, Is.Not.Default);
                    Assert.That(result.Title, Is.EqualTo("Test Task"));
                    Assert.That(result.Status, Is.EqualTo(Status.Pending));
                    Assert.That(result.Priority, Is.EqualTo(Priority.Medium));
                    Assert.That(result.CreatedAt, Is.Not.Default);
                });
            }
        }

        [Test]
        public async Task Create_ShouldReturnTaskItemWithSameId_WhenTaskIsProvided()
        {
            // Arrange
            var guid = Guid.NewGuid();

            var taskItem = new TaskItem
            {
                Id = guid,
                Title = "Test Task",
                Status = Status.Pending,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow
            };

            _repository.Create(Arg.Any<TaskItem>()).Returns(taskItem);

            // Act
            var result = await _repository.Create(taskItem);

            // Assert
            Assert.That(result.Id, Is.EqualTo(guid));
        }

        [Test]
        public async Task GetById_ShouldReturnTaskItem_WhenTaskExists()
        {
            // Arrange
            var guid = Guid.NewGuid();

            var taskItem = new TaskItem
            {
                Id = guid,
                Title = "Test Task",
                Status = Status.Pending,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow
            };

            _repository.GetById(guid).Returns(taskItem);

            // Act
            var result = await _repository.GetById(guid);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(guid));
        }

        [Test]
        public async Task GetById_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repository.GetById(Arg.Any<Guid>())!.Returns((TaskItem?)null);

            // Act
            var result = await _repository.GetById(id);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAll_ShouldReturnAllTaskItems_WhenTasksExist()
        {
            // Arrange
            var tasks = new List<TaskItem>
            {
                new() {
                    Id = Guid.NewGuid(),
                    Title = "Task 1",
                    Status = Status.Pending,
                    Priority = Priority.Medium,
                    CreatedAt = DateTime.UtcNow
                },
                new() {
                    Id = Guid.NewGuid(),
                    Title = "Task 2",
                    Status = Status.InProgress,
                    Priority = Priority.High,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _repository.GetAll().Returns(tasks);

            // Act
            var result = await _repository.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<IEnumerable<TaskItem>>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Count(), Is.EqualTo(2));
                Assert.That(result, Is.EquivalentTo(tasks));
            }
        }

        [Test]
        public async Task GetAll_ShouldReturnEmptyCollection_WhenNoTasksExist()
        {
            // Arrange
            _repository.GetAll().Returns([]);

            // Act
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
            var guid = Guid.NewGuid();
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

            _repository.Update(Arg.Any<TaskItem>()).Returns(taskItem);

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
        public void Update_ShouldThrowException_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Non-existent Task",
                Status = Status.Pending,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow
            };
            _repository.Update(Arg.Any<TaskItem>()).Throws(new KeyNotFoundException("Task not found"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _repository.Update(taskItem));
            Assert.That(ex.Message, Is.EqualTo("Task not found"));
        }

        [Test]
        public async Task Delete_ShouldReturnTrue_WhenTaskIsDeleted()
        {
            // Arrange
            var guid = Guid.NewGuid();
            _repository.Delete(guid).Returns(true);

            // Act
            var result = await _repository.Delete(guid);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task Delete_ShouldReturnFalse_WhenTaskDoesNotExist()
        {
            // Arrange
            var guid = Guid.NewGuid();
            _repository.Delete(guid).Returns(false);

            // Act
            var result = await _repository.Delete(guid);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
