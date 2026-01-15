using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SRSS.Project.Infrastructure.Configurations
{
    public class ProjectMemberConfiguration : IEntityTypeConfiguration<Domain.Entities.ProjectMember>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.ProjectMember> builder)
        {
            builder.ToTable("project_members");

            builder.HasKey(pm => pm.Id);

            builder.Property(pm => pm.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(pm => pm.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();

            builder.Property(pm => pm.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(pm => pm.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(pm => pm.JoinedAt)
                .HasColumnName("joined_at")
                .IsRequired();

            builder.Property(pm => pm.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(pm => pm.ModifiedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            // Relationships
            builder.HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(pm => pm.ProjectId);
            builder.HasIndex(pm => pm.UserId);
        }
    }
}