using ETR.ToDo.Core.Entities.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETR.ToDo.Core.EF.Configurations;

public class ApiLogConfiguration : IEntityTypeConfiguration<ApiLog>
{
    public void Configure(EntityTypeBuilder<ApiLog> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(e => e.ExecutionTime)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(e => e.ExecutionTime)
            .HasDatabaseName("IX_ApiLogs_ExecutionTime");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_ApiLogs_UserId");

        builder.HasIndex(e => e.StatusCode)
            .HasDatabaseName("IX_ApiLogs_StatusCode");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
