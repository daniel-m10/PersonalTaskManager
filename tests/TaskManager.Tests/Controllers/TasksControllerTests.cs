using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskManager.Api.Controllers;
using TaskManager.Api.DTOs;
using TaskManager.Api.Mapping;
using TaskManager.Core.Common;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Services.Interfaces;

namespace TaskManager.Tests.Controllers
{
    [TestFixture]
    public class TasksControllerTests
    {
        private ITaskService _service;
        private TasksController _controller;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var loggerFactory = LoggerFactory.Create(builder => { builder.AddDebug(); });
            var mapperConfig = new MapperConfiguration(
                cfg => cfg.AddProfile(new TaskMappingProfile()),
                loggerFactory
            );
            _mapper = mapperConfig.CreateMapper();
            _service = Substitute.For<ITaskService>();
            _controller = new TasksController(_service, _mapper);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var tasks = GetSampleTasks();
            _service.GetAllAsync().Returns(Task.FromResult(Result<IEnumerable<TaskItem>>.Success(tasks)));

            var responseDtos = tasks.Select(_mapper.Map<TaskResponseDto>).ToList();

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = result as OkObjectResult;

            Assert.That(okResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(okResult.StatusCode, Is.EqualTo(200));
                Assert.That(okResult.Value, Is.EqualTo(responseDtos));
                Assert.That((okResult.Value as IEnumerable<TaskResponseDto>)?.Count() ?? 0, Is.EqualTo(tasks.Count()));
            }
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnInternalServerError_WhenServiceReturnsFailure()
        {
            // Arrange
            var errors = new List<string> { "Some error occurred" };
            _service.GetAllAsync().Returns(Task.FromResult(Result<IEnumerable<TaskItem>>.Failure(errors)));

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;

            Assert.That(objectResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(objectResult.StatusCode, Is.EqualTo(500));
                Assert.That(objectResult.Value, Is.EqualTo(errors));
                Assert.That((objectResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var task = GetSampleTasks().First();
            _service.GetByIdAsync(task.Id).Returns(Task.FromResult(Result<TaskItem?>.Success(task)));

            var responseDto = _mapper.Map<TaskResponseDto>(task);

            // Act
            var result = await _controller.GetByIdAsync(task.Id);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = result as OkObjectResult;

            Assert.That(okResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(okResult.StatusCode, Is.EqualTo(200));
                Assert.That(okResult.Value, Is.EqualTo(responseDto));
            }
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenServiceReturnsNotFound()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var errors = new List<string> { "Task not found" };
            _service.GetByIdAsync(taskId).Returns(Task.FromResult(Result<TaskItem?>.Failure(errors)));

            // Act
            var result = await _controller.GetByIdAsync(taskId);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;

            Assert.That(notFoundResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
                Assert.That(notFoundResult.Value, Is.EqualTo(errors));
                Assert.That((notFoundResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnBadRequest_WhenServiceReturnsValidationError()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var errors = new List<string> { "There were issues with validation." };
            _service.GetByIdAsync(taskId).Returns(Task.FromResult(Result<TaskItem?>.Failure(errors)));

            // Act
            var result = await _controller.GetByIdAsync(taskId);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
                Assert.That(badRequestResult.Value, Is.EqualTo(errors));
                Assert.That((badRequestResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnInternalServerError_WhenServiceReturnsFailure()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var errors = new List<string> { "Some error occurred" };
            _service.GetByIdAsync(taskId).Returns(Task.FromResult(Result<TaskItem?>.Failure(errors)));

            // Act
            var result = await _controller.GetByIdAsync(taskId);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(objectResult.StatusCode, Is.EqualTo(500));
                Assert.That(objectResult.Value, Is.EqualTo(errors));
                Assert.That((objectResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task CreateAsync_ShouldReturnCreated_WhenServiceReturnsSuccess()
        {
            // Arrange
            var task = GetSampleTasks().First();
            _service.CreateAsync(Arg.Any<TaskItem>()).Returns(Task.FromResult(Result<TaskItem?>.Success(task)));

            var createDto = new TaskCreateDto(
                Title: task.Title,
                Description: task.Description,
                Status: task.Status,
                Priority: task.Priority,
                DueDate: task.DueDate
            );

            var responseDto = _mapper.Map<TaskResponseDto>(task);

            // Act
            var result = await _controller.CreateAsync(createDto);

            // Assert
            Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
            var createdResult = result as CreatedAtActionResult;

            Assert.That(createdResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(createdResult.StatusCode, Is.EqualTo(201));
                Assert.That(createdResult.Value, Is.EqualTo(responseDto));
                Assert.That(createdResult.ActionName, Is.EqualTo(nameof(TasksController.GetByIdAsync)));
                Assert.That(createdResult.RouteValues?["id"], Is.EqualTo(task.Id));
            }
        }

        [Test]
        public async Task CreateAsync_ShouldReturnBadRequest_WhenServiceReturnsValidationError()
        {
            // Arrange
            var task = GetSampleTasks().First();
            var errors = new List<string> { "There were issues with validation." };
            _service.CreateAsync(Arg.Any<TaskItem>()).Returns(Task.FromResult(Result<TaskItem?>.Failure(errors)));

            var dto = new TaskCreateDto(
                Title: task.Title,
                Description: task.Description,
                Status: task.Status,
                Priority: task.Priority,
                DueDate: task.DueDate
            );

            // Act
            var result = await _controller.CreateAsync(dto);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
                Assert.That(badRequestResult.Value, Is.EqualTo(errors));
                Assert.That((badRequestResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task CreateAsync_ShouldReturnInternalServerError_WhenServiceReturnsFailure()
        {
            // Arrange
            var task = GetSampleTasks().First();
            var errors = new List<string> { "Some error occurred" };
            _service.CreateAsync(Arg.Any<TaskItem>()).Returns(Task.FromResult(Result<TaskItem?>.Failure(errors)));

            var dto = new TaskCreateDto(
                Title: task.Title,
                Description: task.Description,
                Status: task.Status,
                Priority: task.Priority,
                DueDate: task.DueDate
            );

            // Act
            var result = await _controller.CreateAsync(dto);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;

            Assert.That(objectResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(objectResult.StatusCode, Is.EqualTo(500));
                Assert.That(objectResult.Value, Is.EqualTo(errors));
                Assert.That((objectResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var task = GetSampleTasks().First();
            _service.UpdateAsync(Arg.Any<TaskItem>()).Returns(Task.FromResult(Result<TaskItem?>.Success(task)));

            var updateDto = new TaskUpdateDto(
                Title: task.Title,
                Description: task.Description,
                Status: task.Status,
                Priority: task.Priority,
                DueDate: task.DueDate,
                CompletedAt: task.CompletedAt,
                UpdatedAt: task.UpdatedAt ?? DateTime.UtcNow
            );

            var responseDto = _mapper.Map<TaskResponseDto>(task);

            // Act
            var result = await _controller.UpdateAsync(task.Id, updateDto);

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var okResult = result as OkObjectResult;

            Assert.That(okResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(okResult.StatusCode, Is.EqualTo(200));
                Assert.That(okResult.Value, Is.EqualTo(responseDto));
            }
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnNotFound_WhenServiceReturnsNotFound()
        {
            // Arrange
            var task = GetSampleTasks().First();
            var errors = new List<string> { "Task not found" };
            _service.UpdateAsync(Arg.Any<TaskItem>()).Returns(Task.FromResult(Result<TaskItem?>.Failure(errors)));

            var dto = new TaskUpdateDto(
                Title: task.Title,
                Description: task.Description,
                Status: task.Status,
                Priority: task.Priority,
                DueDate: task.DueDate,
                CompletedAt: task.CompletedAt,
                UpdatedAt: task.UpdatedAt ?? DateTime.UtcNow
            );

            // Act
            var result = await _controller.UpdateAsync(task.Id, dto);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;

            Assert.That(notFoundResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
                Assert.That(notFoundResult.Value, Is.EqualTo(errors));
                Assert.That((notFoundResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnBadRequest_WhenServiceReturnsValidationError()
        {
            // Arrange
            var task = GetSampleTasks().First();
            var errors = new List<string> { "There were issues with validation." };
            _service.UpdateAsync(Arg.Any<TaskItem>()).Returns(Task.FromResult(Result<TaskItem?>.Failure(errors)));

            var dto = new TaskUpdateDto(
                Title: task.Title,
                Description: task.Description,
                Status: task.Status,
                Priority: task.Priority,
                DueDate: task.DueDate,
                CompletedAt: task.CompletedAt,
                UpdatedAt: task.UpdatedAt ?? DateTime.UtcNow
            );

            // Act
            var result = await _controller.UpdateAsync(task.Id, dto);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
                Assert.That(badRequestResult.Value, Is.EqualTo(errors));
                Assert.That((badRequestResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnInternalServerError_WhenServiceReturnsFailure()
        {
            // Arrange
            var task = GetSampleTasks().First();
            var errors = new List<string> { "Some error occurred" };
            _service.UpdateAsync(Arg.Any<TaskItem>()).Returns(Task.FromResult(Result<TaskItem?>.Failure(errors)));

            var dto = new TaskUpdateDto(
                Title: task.Title,
                Description: task.Description,
                Status: task.Status,
                Priority: task.Priority,
                DueDate: task.DueDate,
                CompletedAt: task.CompletedAt,
                UpdatedAt: task.UpdatedAt ?? DateTime.UtcNow
            );

            // Act
            var result = await _controller.UpdateAsync(task.Id, dto);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;

            Assert.That(objectResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(objectResult.StatusCode, Is.EqualTo(500));
                Assert.That(objectResult.Value, Is.EqualTo(errors));
                Assert.That((objectResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task DeleteAsync_ShouldReturnNoContent_WhenServiceReturnsSuccess()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _service.DeleteAsync(taskId).Returns(Task.FromResult(Result<bool>.Success(true)));

            // Act
            var result = await _controller.DeleteAsync(taskId);

            // Assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
            var noContentResult = result as NoContentResult;

            Assert.That(noContentResult, Is.Not.Null);
            Assert.That(noContentResult.StatusCode, Is.EqualTo(204));
        }

        [Test]
        public async Task DeleteAsync_ShouldReturnNotFound_WhenServiceReturnsNotFound()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var errors = new List<string> { "Task not found" };
            _service.DeleteAsync(taskId).Returns(Task.FromResult(Result<bool>.Failure(errors)));

            // Act
            var result = await _controller.DeleteAsync(taskId);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
                Assert.That(notFoundResult.Value, Is.EqualTo(errors));
                Assert.That((notFoundResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task DeleteAsync_ShouldReturnBadRequest_WhenServiceReturnsValidationError()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var errors = new List<string> { "There were issues with validation." };
            _service.DeleteAsync(taskId).Returns(Task.FromResult(Result<bool>.Failure(errors)));

            // Act
            var result = await _controller.DeleteAsync(taskId);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
                Assert.That(badRequestResult.Value, Is.EqualTo(errors));
                Assert.That((badRequestResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task DeleteAsync_ShouldReturnInternalServerError_WhenServiceReturnsFailure()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var errors = new List<string> { "Some error occurred" };
            _service.DeleteAsync(taskId).Returns(Task.FromResult(Result<bool>.Failure(errors)));

            // Act
            var result = await _controller.DeleteAsync(taskId);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;

            Assert.That(objectResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(objectResult.StatusCode, Is.EqualTo(500));
                Assert.That(objectResult.Value, Is.EqualTo(errors));
                Assert.That((objectResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task RestoreAsync_ShouldReturnOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var task = GetSampleTasks().First();
            _service.RestoreAsync(task.Id).Returns(Task.FromResult(Result<bool>.Success(true)));

            // Act
            var result = await _controller.RestoreAsync(task.Id);

            // Assert
            Assert.That(result, Is.TypeOf<OkResult>());
            var okResult = result as OkResult;

            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task RestoreAsync_ShouldReturnNotFound_WhenServiceReturnsNotFound()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var errors = new List<string> { "Task not found" };
            _service.RestoreAsync(taskId).Returns(Task.FromResult(Result<bool>.Failure(errors)));

            // Act
            var result = await _controller.RestoreAsync(taskId);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFoundResult = result as NotFoundObjectResult;

            Assert.That(notFoundResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
                Assert.That(notFoundResult.Value, Is.EqualTo(errors));
                Assert.That((notFoundResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task RestoreAsync_ShouldReturnBadRequest_WhenServiceReturnsValidationError()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var errors = new List<string> { "There were issues with validation." };
            _service.RestoreAsync(taskId).Returns(Task.FromResult(Result<bool>.Failure(errors)));

            // Act
            var result = await _controller.RestoreAsync(taskId);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
                Assert.That(badRequestResult.Value, Is.EqualTo(errors));
                Assert.That((badRequestResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        [Test]
        public async Task RestoreAsync_ShouldReturnInternalServerError_WhenServiceReturnsFailure()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var errors = new List<string> { "Some error occurred" };
            _service.RestoreAsync(taskId).Returns(Task.FromResult(Result<bool>.Failure(errors)));

            // Act
            var result = await _controller.RestoreAsync(taskId);

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var objectResult = result as ObjectResult;

            Assert.That(objectResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(objectResult.StatusCode, Is.EqualTo(500));
                Assert.That(objectResult.Value, Is.EqualTo(errors));
                Assert.That((objectResult.Value as IEnumerable<string>)?.Count() ?? 0, Is.EqualTo(errors.Count));
            }
        }

        private static IEnumerable<TaskItem> GetSampleTasks() =>
            [
                new() {
                    Id = Guid.NewGuid(),
                    Title = "Task 1",
                    Description = "Sample Task 1",
                    Status = Status.Todo,
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    Priority = Priority.Medium,
                    IsDeleted = false
                },
                new() {
                    Id = Guid.NewGuid(),
                    Title = "Task 2",
                    Description = "Sample Task 2",
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(3),
                    Status = Status.InProgress,
                    Priority = Priority.High,
                    IsDeleted = false
                }
            ];
    }
}
