using TaskManager.Api.Mapping;
using TaskManager.Services.Interfaces;
using TaskManager.Services;
using TaskManager.Core.Interfaces;
using TaskManager.Data.Repositories;
using TaskManager.Services.Validators;
using TaskManager.Core.Entities;
using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using TaskManager.Data.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<TaskMappingProfile>());
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Register database connection
builder.Services.AddSingleton<IDbConnection>(provider =>
{
    // Register custom type handler for Guid
    SqlMapper.AddTypeHandler(new GuidTypeHandler());

    var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();

    // Create table schema for Tasks
    var createTableCmd = connection.CreateCommand();
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

    return connection;
});

// Register application services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<TaskManager.Services.Interfaces.IValidator<TaskItem>, TaskItemValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }
