using ETR.ToDo.Core.Entities.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETR.ToDo.Core.EF.Configurations;

public class TaskListConfiguration : IEntityTypeConfiguration<TaskList>
{
    public void Configure(EntityTypeBuilder<TaskList> builder)
    {
        builder.HasKey(tl => tl.Id);
        builder.Property(tl => tl.Id).ValueGeneratedOnAdd();

        // Title: [Required][MinLength(1)][MaxLength] on entity handles schema + validation

        builder.Property(tl => tl.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(tl => tl.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(tl => tl.Order)
            .HasDefaultValue(1);

        builder.HasIndex(tl => new { tl.UserId, tl.IsDeleted })
            .HasDatabaseName("IX_TaskLists_UserId_IsDeleted");

        builder.HasIndex(tl => new { tl.UserId, tl.Order })
            .HasDatabaseName("IX_TaskLists_UserId_Order");

        builder.HasOne(tl => tl.User)
            .WithMany()
            .HasForeignKey(tl => tl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(tl => !tl.IsDeleted);
    }
}
