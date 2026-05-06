using ETR.ToDo.Core.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETR.ToDo.Core.EF.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id).ValueGeneratedOnAdd();

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(256);

        // CreatedAt set by application logic when creating token
        builder.Property(rt => rt.IsRevoked)
            .HasDefaultValue(false);

        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        builder.HasIndex(rt => new { rt.UserId, rt.IsRevoked })
            .HasDatabaseName("IX_RefreshTokens_UserId_IsRevoked");

        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
