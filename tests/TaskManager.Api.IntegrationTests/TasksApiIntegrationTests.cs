using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;
using TaskManager.Api.DTOs;
using TaskManager.Core.Enums;

namespace TaskManager.Api.IntegrationTests;

[TestFixture]
public class TasksApiIntegrationTests
{
    private HttpClient _client;
    private WebApplicationFactory<Program> _factory;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task CreateTask_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new
        {
            Title = "Test Task",
            Description = "This is a test task",
            Status = Status.Todo,
            Priority = Priority.High,
            DueDate = (DateTime?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdTask = await response.Content.ReadFromJsonAsync<TaskResponseDto>();

        Assert.That(createdTask, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdTask.Title, Is.EqualTo(createDto.Title));
            Assert.That(createdTask.Description, Is.EqualTo(createDto.Description));
            Assert.That(createdTask.Status, Is.EqualTo(createDto.Status));
            Assert.That(createdTask.Priority, Is.EqualTo(createDto.Priority));
            Assert.That(createdTask.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdTask.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        }
    }
}
