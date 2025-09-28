using NSubstitute;
using TaskManager.Core.Common;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Core.Interfaces;
using TaskManager.Services;
using TaskManager.Services.Interfaces;
using TaskManager.Services.Validators.Constants;

namespace TaskManager.Tests.Services
{
    [TestFixture]
    public class TaskServiceTests
    {
        private ITaskRepository _repository;
        private IValidator<TaskItem> _validator;

        [SetUp]
        public void Setup()
        {
            _repository = Substitute.For<ITaskRepository>();
            _validator = Substitute.For<IValidator<TaskItem>>();
        }

        [Test]
        public async Task CreateAsync_ShouldReturnSuccess_WhenTaskItemIsValid()
        {
            // Arrange
            var validTask = CreateValidTaskItem();

            _validator.ValidateAsync(Arg.Any<TaskItem>())
                .Returns(new FluentValidation.Results.ValidationResult());
            _repository.Create(Arg.Any<TaskItem>())
                .Returns(callInfo => Task.FromResult(Result<TaskItem>.Success(validTask)));

            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.CreateAsync(validTask);

            // Assert
            await _validator.Received(1).ValidateAsync(validTask);
            await _repository.Received(1).Create(validTask);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(validTask));
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task CreateAsync_ShouldReturnFailure_WhenTaskItemIsInvalid()
        {
            // Arrange
            var invalidTask = CreateInvalidTaskItem();
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new("Title", "Title is required"),
                new("Status", "Invalid status value")
            };
            _validator.ValidateAsync(Arg.Any<TaskItem>())
                .Returns(new FluentValidation.Results.ValidationResult(validationFailures));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.CreateAsync(invalidTask);

            // Assert
            await _validator.Received(1).ValidateAsync(invalidTask);
            await _repository.DidNotReceive().Create(Arg.Any<TaskItem>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Is.EquivalentTo(validationFailures.Select(vf => vf.ErrorMessage)));
            }
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnSuccess_WithActiveTasks()
        {
            // Arrange
            var tasks = CreateTaskItemList(3);
            _repository.GetAll()
                .Returns(callInfo => Task.FromResult(Result<IEnumerable<TaskItem>>.Success(tasks)));

            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            await _repository.Received(1).GetAll();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EquivalentTo(tasks));
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnSuccess_WithEmptyList_WhenNoTasksExist()
        {
            // Arrange
            _repository.GetAll()
                .Returns(callInfo => Task.FromResult(Result<IEnumerable<TaskItem>>.Success(new List<TaskItem>())));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            await _repository.Received(1).GetAll();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.Empty);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnSuccess_WhenTaskExistsAndIsActive()
        {
            // Arrange
            var task = CreateValidTaskItem();
            _repository.GetById(Arg.Any<Guid>())
                .Returns(callInfo => Task.FromResult(Result<TaskItem>.Success(task)));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.GetByIdAsync(task.Id);

            // Assert
            await _repository.Received(1).GetById(task.Id);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(task));
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnFailure_WhenTaskDoesNotExist()
        {
            // Arrange
            _repository.GetById(Arg.Any<Guid>())
                .Returns(callInfo => Task.FromResult(Result<TaskItem>.Failure(ValidationMessages.TaskNotFound)));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.GetByIdAsync(Guid.NewGuid());

            // Assert
            await _repository.Received(1).GetById(Arg.Any<Guid>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Does.Contain(ValidationMessages.TaskNotFound));
            }
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnFailure_WhenTaskIsDeleted()
        {
            // Arrange
            _repository.GetById(Arg.Any<Guid>())
                .Returns(callInfo => Task.FromResult(Result<TaskItem>.Failure(ValidationMessages.TaskNotFound)));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.GetByIdAsync(Guid.NewGuid());

            // Assert
            await _repository.Received(1).GetById(Arg.Any<Guid>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Does.Contain(ValidationMessages.TaskNotFound));
            }
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnSuccess_WhenTaskItemIsValidAndActive()
        {
            // Arrange
            var validTask = CreateValidTaskItem();
            _validator.ValidateAsync(Arg.Any<TaskItem>())
                .Returns(new FluentValidation.Results.ValidationResult());
            _repository.Update(Arg.Any<TaskItem>())
                .Returns(callInfo => Task.FromResult(Result<TaskItem>.Success(validTask)));

            // Simulate that the task exists and is active
            _repository.GetById(validTask.Id)
                .Returns(callInfo => Task.FromResult(Result<TaskItem>.Success(validTask)));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.UpdateAsync(validTask);

            // Assert
            await _validator.Received(1).ValidateAsync(validTask);
            await _repository.Received(1).GetById(validTask.Id);
            await _repository.Received(1).Update(validTask);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EqualTo(validTask));
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnFailure_WhenTaskItemIsInvalid()
        {
            // Arrange
            var invalidTask = CreateInvalidTaskItem();
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new("Title", "Title is required"),
                new("Status", "Invalid status value")
            };

            _repository.GetById(Arg.Any<Guid>())
                .Returns(callInfo => Task.FromResult(Result<TaskItem>.Success(CreateValidTaskItem()))); // Simulate task exists
            _validator.ValidateAsync(Arg.Any<TaskItem>())
                .Returns(new FluentValidation.Results.ValidationResult(validationFailures));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.UpdateAsync(invalidTask);

            // Assert
            await _validator.Received(1).ValidateAsync(invalidTask);
            await _repository.DidNotReceive().Update(Arg.Any<TaskItem>());
            await _repository.Received(1).GetById(Arg.Any<Guid>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Is.EquivalentTo(validationFailures.Select(vf => vf.ErrorMessage)));
            }
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnFailure_WhenTaskDoesNotExist()
        {
            // Arrange
            var validTask = CreateValidTaskItem();
            _repository.GetById(Arg.Any<Guid>())
                .Returns(callInfo => Task.FromResult(Result<TaskItem>.Failure(ValidationMessages.TaskCannotBeUpdated))); // Simulate task does not exist

            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.UpdateAsync(validTask);

            // Assert
            await _repository.Received(1).GetById(validTask.Id);
            await _validator.DidNotReceive().ValidateAsync(Arg.Any<TaskItem>());
            await _repository.DidNotReceive().Update(Arg.Any<TaskItem>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Does.Contain(ValidationMessages.TaskCannotBeUpdated));
            }
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnFailure_WhenTaskIsDeleted()
        {
            // Arrange
            var validTask = CreateValidTaskItem();
            _repository.GetById(Arg.Any<Guid>())
                .Returns(callInfo => Task.FromResult(Result<TaskItem>.Failure(ValidationMessages.TaskCannotBeUpdated))); // Simulate task is deleted

            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.UpdateAsync(validTask);

            // Assert
            await _repository.Received(1).GetById(validTask.Id);
            await _validator.DidNotReceive().ValidateAsync(Arg.Any<TaskItem>());
            await _repository.DidNotReceive().Update(Arg.Any<TaskItem>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.Null);
                Assert.That(result.Errors, Does.Contain(ValidationMessages.TaskCannotBeUpdated));
            }
        }

        [Test]
        public async Task DeleteAsync_ShouldReturnSuccess_WhenTaskIsDeleted()
        {
            // Arrange
            _repository.Delete(Arg.Any<Guid>())
                .Returns(callInfo => Task.FromResult(Result<bool>.Success(true)));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.DeleteAsync(Guid.NewGuid());

            // Assert
            await _repository.Received(1).Delete(Arg.Any<Guid>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.True);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task DeleteAsync_ShouldReturnFailure_WhenTaskDoesNotExist()
        {
            // Arrange
            _repository.Delete(Arg.Any<Guid>())
                .Returns(callInfo => Task.FromResult(Result<bool>.Failure(ValidationMessages.TaskNotFound)));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.DeleteAsync(Guid.NewGuid());

            // Assert
            await _repository.Received(1).Delete(Arg.Any<Guid>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.False);
                Assert.That(result.Errors, Does.Contain(ValidationMessages.TaskNotFound));
            }
        }

        [Test]
        public async Task RestoreAsync_ShouldReturnSuccess_WhenTaskIsRestored()
        {
            // Arrange
            _repository.Restore(Arg.Any<Guid>())
                .Returns(callInfo => Task.FromResult(Result<bool>.Success(true)));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.RestoreAsync(Guid.NewGuid());

            // Assert
            await _repository.Received(1).Restore(Arg.Any<Guid>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.True);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task RestoreAsync_ShouldReturnFailure_WhenTaskDoesNotExist()
        {
            // Arrange
            _repository.Restore(Arg.Any<Guid>())
                .Returns(callInfo => Task.FromResult(Result<bool>.Failure(ValidationMessages.TaskNotFound)));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.RestoreAsync(Guid.NewGuid());

            // Assert
            await _repository.Received(1).Restore(Arg.Any<Guid>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.False);
                Assert.That(result.Errors, Does.Contain(ValidationMessages.TaskNotFound));
            }
        }

        [Test]
        public async Task RestoreAsync_ShouldReturnFailure_WhenTaskIsNotDeleted()
        {
            // Arrange
            _repository.Restore(Arg.Any<Guid>())
                .Returns(callInfo => Task.FromResult(Result<bool>.Failure(ValidationMessages.TaskNotFound)));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.RestoreAsync(Guid.NewGuid());

            // Assert
            await _repository.Received(1).Restore(Arg.Any<Guid>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Value, Is.False);
                Assert.That(result.Errors, Does.Contain(ValidationMessages.TaskNotFound));
            }
        }

        [Test]
        public async Task GetDeletedAsync_ShouldReturnSuccess_WithDeletedTasks()
        {
            // Arrange
            var deletedTasks = CreateTaskItemList(2);
            _repository.GetDeleted()
                .Returns(callInfo => Task.FromResult(Result<IEnumerable<TaskItem>>.Success(deletedTasks)));

            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.GetDeletedAsync();

            // Assert
            await _repository.Received(1).GetDeleted();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.EquivalentTo(deletedTasks));
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task GetDeletedAsync_ShouldReturnSuccess_WithEmptyList_WhenNoDeletedTasksExist()
        {
            // Arrange
            _repository.GetDeleted()
                .Returns(callInfo => Task.FromResult(Result<IEnumerable<TaskItem>>.Success(new List<TaskItem>())));
            var service = new TaskService(_repository, _validator);

            // Act
            var result = await service.GetDeletedAsync();

            // Assert
            await _repository.Received(1).GetDeleted();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.Empty);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        private static TaskItem CreateValidTaskItem() => new()
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "This is a test task",
            Status = Status.Todo,
            Priority = Priority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        private static TaskItem CreateInvalidTaskItem() => new()
        {
            Id = Guid.NewGuid(),
            Title = "", // Invalid: Title is required
            Description = "This is a test task",
            Status = (Status)999, // Invalid: Not a valid enum value
            Priority = Priority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        private static List<TaskItem> CreateTaskItemList(int count)
        {
            var tasks = new List<TaskItem>();
            for (int i = 0; i < count; i++)
            {
                tasks.Add(CreateValidTaskItem());
            }
            return tasks;
        }
    }
}
