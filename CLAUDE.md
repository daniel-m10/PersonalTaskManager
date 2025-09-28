# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Architecture

This is a .NET 9.0 task management application using a clean architecture pattern with the following layers:

- **TaskManager.Api**: ASP.NET Core Web API layer (entry point)
- **TaskManager.Services**: Business logic and service layer
- **TaskManager.Core**: Domain entities, interfaces, and enums
- **TaskManager.Data**: Data access layer with repositories and handlers
- **TaskManager.Tests**: NUnit test project

### Key Domain Model

The core entity is `TaskItem` located in `src/TaskManager.Core/Entities/TaskItem.cs` with:

- Required fields: Id, Title, Status, Priority, CreatedAt
- Optional fields: Description, DueDate, CompletedAt, UpdatedAt
- Uses `Status` and `Priority` enums from `src/TaskManager.Core/Enums/`

The main data contract is defined by `ITaskRepository` in `src/TaskManager.Core/Interfaces/ITaskRepository.cs` providing CRUD operations.

## Development Commands

### Build and Test

```bash
# Build the entire solution
dotnet build

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run a specific test class
dotnet test --filter "ClassName=TaskRepositoryTests"
```

### Run the API

```bash
# Run the API in development mode
dotnet run --project src/TaskManager.Api

# The API runs on:
# HTTP: http://localhost:5010
# HTTPS: https://localhost:7245
```

### Project Management

```bash
# Add package to a specific project
dotnet add src/TaskManager.Api package PackageName

# Add project reference
dotnet add src/ProjectA reference src/ProjectB

# Clean and rebuild
dotnet clean && dotnet build
```

## Code Conventions

- Uses .NET 9.0 with nullable reference types enabled
- Implicit usings enabled across all projects
- NUnit for testing with NSubstitute for mocking
- FluentValidation for input validation in API layer
- Serilog for logging
- Entity regions are used in domain models (Required Fields, Optional Fields)

## Testing

- Test project uses NUnit framework
- Located in `tests/TaskManager.Tests/`
- Uses NSubstitute for mocking dependencies
- Test class example: `TaskRepositoryTests` in `tests/TaskManager.Tests/Repositories/`
- Coverage collection is available with coverlet

## Current Development Status

The TaskItem implementation is fully completed with:

- ✅ Core domain entities and interfaces completed
- ✅ Repository pattern fully implemented with soft delete support
- ✅ Service layer with validation and business logic
- ✅ Comprehensive test coverage (48 tests passing)
- ✅ Data layer implementation with Dapper and SQLite support
- ✅ FluentValidation integration for input validation
- ✅ Result pattern for consistent error handling

### TaskItem Features Implemented

- CRUD operations (Create, Read, Update, Delete)
- Soft delete with GetDeleted() and Restore() methods
- Field validation (Title, Description, Status, Priority, dates)
- Custom type handlers for Guid in SQLite
