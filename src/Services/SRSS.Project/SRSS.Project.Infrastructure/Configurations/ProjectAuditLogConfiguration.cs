using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.Project.Domain.Entities;

namespace SRSS.Project.Infrastructure.Configurations
{
    public class ProjectAuditLogConfiguration : IEntityTypeConfiguration<ProjectAuditLog>
    {
        public void Configure(EntityTypeBuilder<ProjectAuditLog> builder)
        {
            builder.ToTable("project_audit_logs");

            builder.HasKey(pal => pal.Id);

            builder.Property(pal => pal.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(pal => pal.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();

            builder.Property(pal => pal.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(pal => pal.Action)
                .HasColumnName("action")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(pal => pal.Description)
                .HasColumnName("description")
                .HasColumnType("text");

            builder.Property(pal => pal.OldValue)
                .HasColumnName("old_value")
                .HasColumnType("jsonb");

            builder.Property(pal => pal.NewValue)
                .HasColumnName("new_value")
                .HasColumnType("jsonb");

            builder.Property(pal => pal.PerformedAt)
                .HasColumnName("performed_at")
                .IsRequired();

            // Relationships
            builder.HasOne(pal => pal.Project)
                .WithMany()
                .HasForeignKey(pal => pal.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for better query performance
            builder.HasIndex(pal => pal.ProjectId);
            builder.HasIndex(pal => pal.UserId);
            builder.HasIndex(pal => pal.PerformedAt);
        }
    }
}
