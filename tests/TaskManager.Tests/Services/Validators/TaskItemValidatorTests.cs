using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Services.Validators;

namespace TaskManager.Tests.Services.Validators
{
    [TestFixture]
    public class TaskItemValidatorTests
    {
        private TaskItemValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new TaskItemValidator();
        }

        [Test]
        public async Task ValidateAsync_ShouldPass_WhenTaskItemIsValid()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow,
                Description = "This is a valid description.",
                DueDate = DateTime.UtcNow.AddDays(5),
                CompletedAt = null,
                UpdatedAt = null
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldFail_WhenTitleIsEmpty()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = string.Empty,
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            var errors = result.Errors.FindAll(e => e.ErrorMessage.Contains("Title")).ToList();
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsValid, Is.False);
                Assert.That(errors, Has.Count.EqualTo(1));
                Assert.That(errors[0].ErrorMessage, Is.EqualTo("Title is required."));
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldFail_WhenTitleExceedsMaxLength()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = new string('A', 101), // 101 characters
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            var errors = result.Errors.FindAll(e => e.ErrorMessage.Contains("Title")).ToList();

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsValid, Is.False);
                Assert.That(errors, Has.Count.EqualTo(1));
                Assert.That(errors[0].ErrorMessage, Is.EqualTo("Title cannot exceed 100 characters."));
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldFail_WhenStatusIsInvalid()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = (Status)999, // Invalid status
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            var errors = result.Errors.FindAll(e => e.ErrorMessage.Contains("status")).ToList();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(errors, Has.Count.EqualTo(1));
                Assert.That(errors[0].ErrorMessage, Is.EqualTo("Invalid status value."));
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldFail_WhenPriorityIsInvalid()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = (Priority)999, // Invalid priority
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            var errors = result.Errors.FindAll(e => e.ErrorMessage.Contains("priority")).ToList();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(errors, Has.Count.EqualTo(1));
                Assert.That(errors[0].ErrorMessage, Is.EqualTo("Invalid priority value."));
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldFail_WhenCreatedAtIsDefault()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = default // Invalid CreatedAt
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            var errors = result.Errors.FindAll(e => e.ErrorMessage.Contains("CreatedAt")).ToList();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(errors, Has.Count.EqualTo(1));
                Assert.That(errors[0].ErrorMessage, Is.EqualTo("CreatedAt is required."));
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldFail_WhenDueDateIsInThePast()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow,
                DueDate = DateTime.Now.AddDays(-1) // Past date
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            var errors = result.Errors.FindAll(e => e.ErrorMessage.Contains("Due date")).ToList();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(errors, Has.Count.EqualTo(1));
                Assert.That(errors[0].ErrorMessage, Is.EqualTo("Due date cannot be in the past."));
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldFail_WhenDescriptionExceedsMaxLength()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow,
                Description = new string('A', 501) // 501 characters
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            var errors = result.Errors.FindAll(e => e.ErrorMessage.Contains("Description")).ToList();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(errors, Has.Count.EqualTo(1));
                Assert.That(errors[0].ErrorMessage, Is.EqualTo("Description cannot exceed 500 characters."));
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldFail_WhenCompletedAtIsBeforeCreatedAt()
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = createdAt,
                CompletedAt = createdAt.AddMinutes(-10) // Before CreatedAt
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            var errors = result.Errors.FindAll(e => e.ErrorMessage.Contains("CompletedAt")).ToList();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(errors, Has.Count.EqualTo(1));
                Assert.That(errors[0].ErrorMessage, Is.EqualTo("CompletedAt must be after CreatedAt."));
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldFail_WhenUpdatedAtIsBeforeCreatedAt()
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = createdAt,
                UpdatedAt = createdAt.AddMinutes(-10) // Before CreatedAt
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            var errors = result.Errors.FindAll(e => e.ErrorMessage.Contains("UpdatedAt")).ToList();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(errors, Has.Count.EqualTo(1));
                Assert.That(errors[0].ErrorMessage, Is.EqualTo("UpdatedAt must be after CreatedAt."));
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldPass_WhenTitleIsExactlyMaxLength()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = new string('A', 100), // Exactly 100 characters
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldPass_WhenDescriptionIsExactlyMaxLength()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow,
                Description = new string('A', 500) // Exactly 500 characters
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldPass_WhenDueDateIsToday()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow,
                DueDate = DateTime.Today // Today
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldPass_WhenDescriptionIsNullOrEmpty()
        {
            // Arrange
            var taskItemWithNullDescription = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow,
                Description = null!
            };

            var taskItemWithEmptyDescription = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow,
                Description = string.Empty
            };

            // Act
            var resultWithNullDescription = await _validator.ValidateAsync(taskItemWithNullDescription);

            var resultWithEmptyDescription = await _validator.ValidateAsync(taskItemWithEmptyDescription);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(resultWithNullDescription.IsValid, Is.True);
                Assert.That(resultWithNullDescription.Errors, Is.Empty);
                Assert.That(resultWithEmptyDescription.IsValid, Is.True);
                Assert.That(resultWithEmptyDescription.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldPass_WhenDueDateIsFarInFuture()
        {
            // Arrange
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddYears(10) // Far future date
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldFail_WhenCompletedAtEqualsCreatedAt()
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = createdAt,
                CompletedAt = createdAt // Equals CreatedAt
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            var errors = result.Errors.FindAll(e => e.ErrorMessage.Contains("CompletedAt")).ToList();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(errors, Has.Count.EqualTo(1));
                Assert.That(errors[0].ErrorMessage, Is.EqualTo("CompletedAt must be after CreatedAt."));
            }
        }

        [Test]
        public async Task ValidateAsync_ShouldFail_WhenUpdatedAtEqualsCreatedAt()
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = "Valid Title",
                Status = Status.Todo,
                Priority = Priority.Low,
                CreatedAt = createdAt,
                UpdatedAt = createdAt // Equals CreatedAt
            };

            // Act
            var result = await _validator.ValidateAsync(taskItem);

            // Assert
            var errors = result.Errors.FindAll(e => e.ErrorMessage.Contains("UpdatedAt")).ToList();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(errors, Has.Count.EqualTo(1));
                Assert.That(errors[0].ErrorMessage, Is.EqualTo("UpdatedAt must be after CreatedAt."));
            }
        }
    }
}
