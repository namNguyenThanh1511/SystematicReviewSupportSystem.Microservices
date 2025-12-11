using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Password).IsRequired();
            builder.Property(u => u.Email).HasMaxLength(100).IsRequired(false);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Username).HasMaxLength(50);
            builder.HasIndex(u => u.Username).IsUnique();
            builder.Property(u => u.FullName).HasMaxLength(100);
            builder.Property(u => u.Role).IsRequired();
            builder.Property(u => u.Role).HasConversion<string>();
            builder.Property(u => u.CreatedAt);
            builder.Property(u => u.ModifiedAt);
        }
    }
}
