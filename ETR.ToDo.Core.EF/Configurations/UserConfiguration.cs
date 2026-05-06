using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Core.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETR.ToDo.Core.EF.Configurations;

/// <summary>
/// Entity configuration for User entity
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table name already defined via [Table("Users")] attribute
        // Annotations already handle Required, MaxLength validation

        // Primary Key
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        // Default Values (CreatedDate handled by DbContext.SaveChangesAsync)
        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.IsSystemUser)
            .HasDefaultValue(false);

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        // Database-Specific Constraints (Can't do with annotations!)

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("IX_Users_IsDeleted");

        builder.HasIndex(u => new { u.FirstName, u.LastName })
            .HasDatabaseName("IX_Users_FullName");

        builder.HasIndex(u => u.Role)
            .HasDatabaseName("IX_Users_Role");

        // Role Configuration - Stored as INT with default value
        builder.Property(u => u.Role)
            .HasDefaultValue(UserRole.Basic);

        // Query Filter for Soft Delete
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
