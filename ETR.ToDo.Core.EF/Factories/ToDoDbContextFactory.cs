using ETR.ToDo.Core.EF.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ETR.ToDo.Core.EF.Factories;

/// <summary>
/// Factory for creating DbContext at design-time (for migrations)
/// This enables EF Core tools to create the DbContext when running commands like:
/// - dotnet ef migrations add
/// - dotnet ef database update
/// </summary>
public class ToDoDbContextFactory : IDesignTimeDbContextFactory<ToDoDbContext>
{
    public ToDoDbContext CreateDbContext(string[] args)
    {
        // For design-time, look for appsettings in the Web.Host project
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "ETR.ToDo.Web.Host");

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Create DbContext options for SQLite
        var optionsBuilder = new DbContextOptionsBuilder<ToDoDbContext>();

        optionsBuilder.UseSqlite(connectionString);

        // Enable sensitive data logging in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        return new ToDoDbContext(optionsBuilder.Options);
    }
}
