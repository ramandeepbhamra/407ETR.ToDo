using ETR.ToDo.Core.Entities.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETR.ToDo.Core.EF.Configurations;

public class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
{
    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        builder.HasKey(tt => tt.Id);
        builder.Property(tt => tt.Id).ValueGeneratedOnAdd();

        // Title: [Required][MinLength(1)][MaxLength] on entity handles schema + validation

        builder.Property(tt => tt.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(tt => tt.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(tt => tt.IsCompleted)
            .HasDefaultValue(false);

        builder.Property(tt => tt.IsFavourite)
            .HasDefaultValue(false);

        builder.Property(tt => tt.Order)
            .HasDefaultValue(1);

        // DueDate maps to SQL Server 'date' type (no time component)
        builder.Property(tt => tt.DueDate)
            .HasColumnType("date");

        builder.HasIndex(tt => new { tt.UserId, tt.IsDeleted })
            .HasDatabaseName("IX_TodoTasks_UserId_IsDeleted");

        builder.HasIndex(tt => new { tt.ListId, tt.Order })
            .HasDatabaseName("IX_TodoTasks_ListId_Order");

        builder.HasIndex(tt => new { tt.UserId, tt.IsFavourite })
            .HasDatabaseName("IX_TodoTasks_UserId_IsFavourite");

        // Cascade from TaskList — when a list is deleted its tasks go too
        builder.HasOne(tt => tt.List)
            .WithMany(tl => tl.Tasks)
            .HasForeignKey(tt => tt.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        // NoAction on User FK — avoids multiple cascade paths
        // (User → TaskList → TodoTask is the cascade path)
        builder.HasOne(tt => tt.User)
            .WithMany()
            .HasForeignKey(tt => tt.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(tt => !tt.IsDeleted);
    }
}
