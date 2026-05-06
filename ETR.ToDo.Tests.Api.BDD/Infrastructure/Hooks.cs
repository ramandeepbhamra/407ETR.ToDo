using Reqnroll;
using Reqnroll.BoDi;

namespace ETR.ToDo.Tests.Api.BDD.Infrastructure;

/// <summary>
/// Hooks for managing test lifecycle events.
/// </summary>
[Binding]
public class Hooks
{
    private static CustomWebApplicationFactory? _factory;
    private readonly IObjectContainer _objectContainer;

    public Hooks(IObjectContainer objectContainer)
    {
        _objectContainer = objectContainer;
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        // Create factory once for all tests
        _factory = new CustomWebApplicationFactory();
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        // Dispose factory after all tests
        _factory?.Dispose();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        // Register factory instance in scenario container
        _objectContainer.RegisterInstanceAs(_factory!);

        // Create and register scenario context
        var scenarioContext = new ScenarioContextHelper
        {
            HttpClient = _factory!.CreateClient()
        };
        _objectContainer.RegisterInstanceAs(scenarioContext);
    }

    [AfterScenario]
    public void AfterScenario()
    {
        // Clean up scenario-specific resources
        var scenarioContext = _objectContainer.Resolve<ScenarioContextHelper>();
        scenarioContext.HttpClient?.Dispose();
        scenarioContext.ClearAuthToken();
    }
}
