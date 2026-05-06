# ETR.ToDo.Tests — Unit Tests

xUnit unit tests for service layer business logic.

**Stack:** xUnit 2.9.3 · Moq 4.20.72 · FluentAssertions 7.0.0 · MockQueryable.Moq 8.0.0

---

## Running Tests

```bash
dotnet test ETR.ToDo.Tests
dotnet test ETR.ToDo.Tests --verbosity detailed
dotnet watch test --project ETR.ToDo.Tests
```

---

## Test Pattern

```csharp
public class TodoTaskServiceTests
{
    private readonly Mock<IRepository<TodoTask, Guid>> _taskRepo = new();

    [Fact]
    public async Task CreateTask_WithValidInput_ReturnsDto()
    {
        // Arrange
        var users = new List<TodoTask>().AsQueryable().BuildMock();
        _taskRepo.Setup(r => r.GetAll()).Returns(users);
        var service = new TodoTaskService(_taskRepo.Object, new TaskMapper());

        // Act
        var result = await service.CreateAsync(Guid.NewGuid(), new CreateTodoTaskDto { Title = "Test" });

        // Assert
        result.Title.Should().Be("Test");
    }
}
```

Rules:
- AAA structure: Arrange / Act / Assert
- `[Fact]` for single cases, `[Theory]` + `[InlineData]` for parameterised
- FluentAssertions always — never `Assert.X()`
- `MockQueryable` to mock `IRepository.GetAll()` with async support
- Mock only direct dependencies of the class under test

---

## Project Structure

```
ETR.ToDo.Tests/
├── Services/
│   ├── AuthenticationServiceTests.cs
│   ├── TodoTaskServiceTests.cs
│   └── ...
└── ETR.ToDo.Tests.csproj
```

Test files co-located by service — one test class per service class.

---

## See Also

- [Adding Features](../docs/development/adding-features.md) — includes service patterns to test
- [qa-engineer agent](../.ai/agents/qa-engineer.md) — full test patterns and MockQueryable examples
