namespace ETR.ToDo.Services.Core.Seeding;

/// <summary>
/// Contract for seeding initial application data on startup.
/// Implementations must be idempotent — safe to call on every application start.
/// </summary>
public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
