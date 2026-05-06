using Xunit;

namespace ETR.ToDo.Tests.Api.BDD.Infrastructure;

/// <summary>
/// Collection definition for API integration tests.
/// Ensures WebApplicationFactory is shared across all tests in the collection.
/// </summary>
[CollectionDefinition("API Integration Tests")]
public class ApiTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // This class is never instantiated.
    // It serves only as a marker for xUnit to share fixtures across tests.
}
