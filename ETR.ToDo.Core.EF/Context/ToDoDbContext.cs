using ETR.ToDo.Core.Entities.Auth;
using ETR.ToDo.Core.Entities.Logging;
using ETR.ToDo.Core.Entities.Tasks;
using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Core.Entities.Base;
using ETR.ToDo.Core.EF.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ETR.ToDo.Core.EF.Context;

/// <summary>
/// Main DbContext for the ToDo application
/// </summary>
public class ToDoDbContext : DbContext
{
    public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<TaskList> TaskLists => Set<TaskList>();
    public DbSet<TodoTask> TodoTasks => Set<TodoTask>();
    public DbSet<SubTask> SubTasks => Set<SubTask>();
    public DbSet<ApiLog> ApiLogs => Set<ApiLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);

        // Additional global configurations can be added here
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set audit fields
        SetAuditFields();

        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        // Automatically set audit fields
        SetAuditFields();

        return base.SaveChanges();
    }

    /// <summary>
    /// Automatically sets audit fields (CreatedDate, ModifiedDate, etc.) before saving changes
    /// </summary>
    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();
        var currentTime = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = currentTime;
                    // CreatedBy should be set from the current user context in the application layer
                    // For now, we'll leave it to be set by the application
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedDate = currentTime;
                    // ModifiedBy should be set from the current user context in the application layer
                    // Prevent overwriting CreatedDate and CreatedBy
                    entry.Property(nameof(IAuditableEntity.CreatedDate)).IsModified = false;
                    entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                    break;
            }
        }

        // Handle soft deletes
        var softDeletableEntries = ChangeTracker.Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entry in softDeletableEntries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedDate = currentTime;
            // DeletedBy should be set from the current user context in the application layer
        }
    }
}
