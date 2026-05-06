using ETR.ToDo.Core.Entities.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETR.ToDo.Core.EF.Configurations;

public class SubTaskConfiguration : IEntityTypeConfiguration<SubTask>
{
    public void Configure(EntityTypeBuilder<SubTask> builder)
    {
        builder.HasKey(st => st.Id);
        builder.Property(st => st.Id).ValueGeneratedOnAdd();

        // Title: [Required][MinLength(1)][MaxLength] on entity handles schema + validation

        builder.Property(st => st.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(st => st.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(st => st.IsCompleted)
            .HasDefaultValue(false);

        builder.HasIndex(st => new { st.TodoTaskId, st.IsDeleted })
            .HasDatabaseName("IX_SubTasks_TodoTaskId_IsDeleted");

        builder.HasOne(st => st.TodoTask)
            .WithMany(tt => tt.SubTasks)
            .HasForeignKey(st => st.TodoTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(st => !st.IsDeleted);
    }
}
