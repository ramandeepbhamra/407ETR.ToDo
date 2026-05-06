# ETR.ToDo.Tests.Api.BDD - API Integration Tests (BDD)

BDD integration test suite for ETR.ToDo REST API using **Reqnroll** with **Gherkin** syntax.

---

## 📋 Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| .NET SDK | 10.x | Required |
| ETR.ToDo.Web.Host | - | Must compile successfully |

---

## 📦 NuGet Dependencies

Automatically installed via project file:

- **Reqnroll** 2.4.0 — BDD framework
- **Reqnroll.xUnit** 2.4.0 — xUnit integration
- **Microsoft.AspNetCore.Mvc.Testing** 10.0.5 — WebApplicationFactory
- **FluentAssertions** 7.0.0 — Assertions
- **Microsoft.EntityFrameworkCore.Sqlite** 10.0.5 — In-memory database
- **BCrypt.Net-Next** 4.0.3 — Password hashing

---

## 🚀 Running Tests

### Command Line

```powershell
# Run all BDD tests
dotnet test ETR.ToDo.Tests.Api.BDD

# With Gherkin step output
dotnet test ETR.ToDo.Tests.Api.BDD --verbosity normal

# Filter specific scenarios
dotnet test ETR.ToDo.Tests.Api.BDD --filter "DisplayName~successful"

# Watch mode (auto-run on changes)
dotnet watch test --project ETR.ToDo.Tests.Api.BDD
```

### Visual Studio

1. Open **Test Explorer**: `Ctrl + E, T`
2. Build solution: `Ctrl + Shift + B`
3. Click ▶️ to run all tests
4. Right-click scenarios to run/debug individually

---

## 🏗️ Project Structure

```
ETR.ToDo.Tests.Api.BDD/
├── Features/                    # Gherkin .feature files
│   └── Authentication.Login.feature
│
├── StepDefinitions/             # Step implementations
│   └── AuthenticationLoginSteps.cs
│
├── Infrastructure/              # Test infrastructure
│   ├── CustomWebApplicationFactory.cs
│   ├── ScenarioContextHelper.cs
│   ├── StepDefinitionBase.cs
│   └── Hooks.cs
│
├── reqnroll.json               # Reqnroll configuration
└── ETR.ToDo.Tests.Api.BDD.csproj
```

---

## 🔧 Configuration

### Test Environment

- **Environment:** `Testing` (skips auto-migration in `Program.cs`)
- **Database:** SQLite in-memory (no files, auto-cleanup)
- **Pre-seeded users:**
  - `testuser@example.com` / `Password123!` (Basic role)
  - `admin@example.com` / `Admin123!` (Admin role)

### How It Works

**CustomWebApplicationFactory** creates an isolated test environment:
- In-memory SQLite database (exists only in RAM)
- Real HTTP server (full ASP.NET Core pipeline)
- Pre-seeded test users
- Automatic cleanup after tests

---

## 📝 Writing New Tests

### 1. Create Feature File

`Features/MyFeature.feature`:
```gherkin
Feature: User Registration
    As a new user
    I want to create an account
    So that I can access the application

Scenario: Successful registration
    Given I am not authenticated
    When I register with email "new@example.com" and password "Pass123!"
    Then the response status code should be 201
    And I should be automatically logged in
```

### 2. Implement Steps

`StepDefinitions/MyFeatureSteps.cs`:
```csharp
[Binding]
public class RegistrationSteps : StepDefinitionBase
{
    public RegistrationSteps(ScenarioContextHelper context, CustomWebApplicationFactory factory)
        : base(context, factory) { }

    [When(@"I register with email ""(.*)"" and password ""(.*)""")]
    public async Task WhenIRegister(string email, string password)
    {
        var request = new RegisterRequestDto { Email = email, Password = password };
        var response = await HttpClient.PostAsJsonAsync("/api/auth/register", request);
        Context.LastResponse = response;
    }
}
```

### 3. Run Tests

```powershell
dotnet test ETR.ToDo.Tests.Api.BDD --filter "DisplayName~Registration"
```

---

## 🐛 Troubleshooting

**Issue:** "Step definition not found"  
**Fix:** Ensure regex pattern exactly matches Gherkin step (case-sensitive)

**Issue:** Tests not discovered in Test Explorer  
**Fix:** Build solution (`Ctrl + Shift + B`)

**Issue:** WebApplicationFactory not starting  
**Fix:** Verify `Program.cs` has `public partial class Program { }`

**Issue:** Database errors  
**Fix:** Check `Program.cs` skips migrations in Testing environment:
```csharp
if (!app.Environment.IsEnvironment("Testing"))
{
    await context.Database.MigrateAsync();
}
```

---

## 📚 Resources

- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [Gherkin Syntax](https://cucumber.io/docs/gherkin/reference/)
- [FluentAssertions](https://fluentassertions.com/)
- [ASP.NET Core Integration Tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
