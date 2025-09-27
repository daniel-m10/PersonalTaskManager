# Project 1: Foundations API - Personal Task Manager

## 🎯 Project Overview

Build a personal task management API that demonstrates core .NET 8 Web API fundamentals, clean architecture principles, and professional development practices. This project establishes your foundation for all subsequent APIs.

**Duration**: 3 weeks  
**Complexity**: Low-Medium (focus on doing it professionally)  
**Repository Name**: `task-manager-api`

## 📋 Functional Requirements

### Core Features

1. **Task CRUD Operations**
   - Create new tasks with title, description, priority, due date
   - Retrieve tasks with filtering (status, priority, due date range)
   - Update task details and status
   - Soft delete tasks (mark as deleted, don't remove from DB)

2. **Task Categories**
   - Create and manage task categories
   - Assign categories to tasks
   - List tasks by category

3. **Task Status Management**
   - Task statuses: Todo, In Progress, Done, Cancelled
   - Track status change timestamps
   - Prevent invalid status transitions

4. **Search and Filtering**
   - Search tasks by title/description
   - Filter by status, priority, category, date range
   - Sort by creation date, due date, priority

## 🏗️ Non-Functional Requirements

### Performance

- API response time: <200ms for simple operations
- Database queries: Optimized with proper indexing
- Pagination: Max 50 items per page

### Reliability

- 99% uptime under normal conditions
- Graceful error handling with meaningful messages
- Request correlation IDs for tracing

### Security

- Input validation on all endpoints
- SQL injection prevention
- HTTPS only in production
- Basic rate limiting (100 requests/minute per IP)

### Observability

- Structured logging with Serilog
- Health check endpoint
- Metrics for request duration and error rates
- Correlation IDs in all log entries

## 🏛️ Architecture Design

### Layer Structure (Simplified Clean Architecture)

```txt
TaskManager.Api/          # Controllers, middleware, startup
├── Controllers/          # API endpoints
├── Middleware/          # Custom middleware
├── Extensions/          # Service configuration
└── Program.cs           # Application entry point

TaskManager.Core/        # Domain entities and interfaces
├── Entities/            # Domain models
├── Interfaces/          # Repository and service contracts
├── Enums/              # Status, Priority enumerations
└── Exceptions/         # Custom domain exceptions

TaskManager.Services/    # Business logic layer
├── Services/           # Business logic implementation
├── DTOs/              # Data transfer objects
├── Validators/        # FluentValidation rules
└── Mappings/          # AutoMapper profiles

TaskManager.Data/       # Data access layer
├── Repositories/      # Data access implementation
├── Configurations/    # Database configuration
├── Migrations/        # Database schema changes
└── Context/          # Database context
```

### Design Rationale

- **Simplified Clean Architecture**: Not full DDD, but proper separation of concerns
- **Dapper over EF Core**: Learn raw SQL skills, better performance for simple operations
- **SQLite**: Zero-configuration database, perfect for learning and demos
- **Repository Pattern**: Abstracts data access, enables easier testing

## 📝 API Endpoints Design

### Tasks Controller

```txt
GET    /api/tasks                    # Get filtered/paginated tasks
POST   /api/tasks                    # Create new task
GET    /api/tasks/{id}              # Get task by ID
PUT    /api/tasks/{id}              # Update task
DELETE /api/tasks/{id}              # Soft delete task
PATCH  /api/tasks/{id}/status       # Update task status only
```

### Categories Controller

```txt
GET    /api/categories              # Get all categories
POST   /api/categories              # Create category
PUT    /api/categories/{id}         # Update category
DELETE /api/categories/{id}         # Delete category (if no tasks assigned)
```

### System Endpoints

```txt
GET    /health                      # Health check
GET    /api/health/detailed         # Detailed health status
```

## 🗄️ Database Schema

### Tables

```sql
-- Categories
CREATE TABLE Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT,
    Color TEXT, -- Hex color for UI
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL
);

-- Tasks
CREATE TABLE Tasks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT,
    Status INTEGER NOT NULL, -- Enum: 0=Todo, 1=InProgress, 2=Done, 3=Cancelled
    Priority INTEGER NOT NULL, -- Enum: 0=Low, 1=Medium, 2=High, 3=Critical
    DueDate DATETIME,
    CategoryId INTEGER,
    IsDeleted BOOLEAN DEFAULT 0,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    CompletedAt DATETIME,
    
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- Task Status History (for audit trail)
CREATE TABLE TaskStatusHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TaskId INTEGER NOT NULL,
    FromStatus INTEGER,
    ToStatus INTEGER NOT NULL,
    ChangedAt DATETIME NOT NULL,
    
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id)
);
```

## ✅ Definition of Done (DoD)

### Code Quality

- [ ] All code follows C# naming conventions
- [ ] SOLID principles applied appropriately
- [ ] No code smells (long methods, large classes)
- [ ] XML documentation for public APIs
- [ ] No hardcoded values (use configuration)

### Testing (Target: 85%+ coverage)

- [ ] Unit tests for all business logic
- [ ] Integration tests for API endpoints
- [ ] Repository tests with in-memory SQLite
- [ ] Validator tests with edge cases
- [ ] All tests use AAA pattern (Arrange, Act, Assert)

### CI/CD Pipeline

- [ ] GitHub Actions workflow configured
- [ ] Build, test, and lint stages
- [ ] Docker image build and push
- [ ] Automated deployment to staging environment
- [ ] Branch protection rules enforced

### Documentation

- [ ] Comprehensive README with setup instructions
- [ ] API documentation with Swagger/OpenAPI
- [ ] Architecture Decision Records (ADRs) for key decisions
- [ ] Database schema documentation
- [ ] Postman collection for API testing

### Production Readiness

- [ ] Dockerized with multi-stage build
- [ ] Health checks implemented
- [ ] Structured logging throughout
- [ ] Error handling middleware
- [ ] Input validation on all endpoints
- [ ] Database connection pooling configured

### Security

- [ ] HTTPS configured
- [ ] Input sanitization
- [ ] SQL injection protection
- [ ] Basic rate limiting
- [ ] Security headers configured

## 🎯 Key Learning Objectives

### Week 1: Project Setup & Core Structure

- Set up .NET 8 Web API project structure
- Configure dependency injection
- Implement basic repository pattern
- Set up SQLite with Dapper
- Create first controller with basic CRUD

### Week 2: Business Logic & Testing

- Implement service layer with business rules
- Add FluentValidation for input validation
- Write comprehensive unit tests
- Add integration tests
- Implement error handling middleware

### Week 3: Production Readiness

- Add structured logging with Serilog
- Implement health checks
- Create Docker configuration
- Set up GitHub Actions CI/CD
- Complete documentation and polish

## 🤖 GitHub Copilot Mentor Prompt

```txt
You are my Backend Mentor for building a Personal Task Manager API using .NET 8. I'm transitioning from SDET to Backend Development.

CONTEXT:
- I'm building a task management API with .NET 8, SQLite, and Dapper
- This is my first backend project focused on clean architecture and professional practices
- I want to learn through TDD and proper Git workflow

YOUR ROLE:
1. Guide me step-by-step WITHOUT giving complete code solutions
2. Ask me design questions before suggesting implementations
3. Enforce TDD practices (write tests first)
4. Remind me of SOLID principles when I'm about to violate them
5. Ensure I follow proper Git workflow (feature branches, PRs, no direct pushes to main)

CURRENT PROJECT PHASE: [Tell me which week/phase you're in]

RULES FOR OUR INTERACTION:
- Always ask "What do you think should happen here?" before solving
- When I ask for code, first ask me to explain the requirements in my own words
- Suggest I write the test first, then guide me to make it pass
- If I'm stuck, give me small hints, not full solutions
- Remind me to commit frequently with good commit messages
- Check that I'm following the repository pattern and dependency injection

TECHNOLOGY STACK:
- .NET 8 Web API
- SQLite database
- Dapper for data access
- NUnit for testing
- NSubstitute for mocking
- FluentValidation for input validation
- Serilog for logging

ARCHITECTURE LAYERS:
- TaskManager.Api (Controllers, Middleware)
- TaskManager.Core (Entities, Interfaces, Exceptions)
- TaskManager.Services (Business Logic, DTOs, Validators)
- TaskManager.Data (Repositories, Database Context)

When I ask for help, first ask me:
1. What specific problem are you trying to solve?
2. What have you tried so far?
3. What test are you currently writing?
4. What does your current implementation look like?

Then guide me to the solution through questions and small hints.

Remember: I learn best by doing, not by reading large code blocks. Make me think!
```
