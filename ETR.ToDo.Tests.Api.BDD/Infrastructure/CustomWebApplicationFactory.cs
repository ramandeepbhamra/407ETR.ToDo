using ETR.ToDo.Core.EF.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ETR.ToDo.Tests.Api.BDD.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// Creates an in-memory SQLite database for each test run.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ToDoDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Create and open SQLite in-memory connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Add in-memory SQLite database for testing
            services.AddDbContext<ToDoDbContext>(options =>
            {
                options.UseSqlite(_connection);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ToDoDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            try
            {
                // Seed test data
                SeedTestData(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database with test data.");
            }
        });

        builder.UseEnvironment("Testing");
    }

    private static void SeedTestData(ToDoDbContext context)
    {
        // Seed test users for authentication tests
        var testUser = new ETR.ToDo.Core.Entities.Users.User
        {
            Id = Guid.NewGuid(),
            Email = "testuser@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = ETR.ToDo.Core.Shared.Enums.UserRole.Basic,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        var adminUser = new ETR.ToDo.Core.Entities.Users.User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = ETR.ToDo.Core.Shared.Enums.UserRole.Admin,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.Users.AddRange(testUser, adminUser);
        context.SaveChanges();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }
}
