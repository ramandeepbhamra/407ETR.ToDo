using Reqnroll;

namespace ETR.ToDo.Tests.Api.BDD.Infrastructure;

/// <summary>
/// Base class for all step definition classes.
/// Provides access to shared test infrastructure.
/// </summary>
[Binding]
public abstract class StepDefinitionBase
{
    protected readonly ScenarioContextHelper Context;
    protected readonly CustomWebApplicationFactory Factory;

    protected StepDefinitionBase(ScenarioContextHelper context, CustomWebApplicationFactory factory)
    {
        Context = context;
        Factory = factory;
    }

    protected HttpClient HttpClient => Context.HttpClient;
}
