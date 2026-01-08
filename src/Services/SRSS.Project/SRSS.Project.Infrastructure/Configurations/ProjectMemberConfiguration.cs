using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SRSS.Project.Infrastructure.Configurations
{
    public class ProjectMemberConfiguration : IEntityTypeConfiguration<Domain.Entities.ProjectMember>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.ProjectMember> builder)
        {
            builder.ToTable("ProjectMembers");
            builder.HasKey(pm => pm.Id);
            builder.Property(pm => pm.ProjectId).IsRequired();
            builder.Property(pm => pm.UserId).IsRequired();
            builder.Property(pm => pm.Role).IsRequired();
            builder.Property(pm => pm.Role).HasConversion<string>();
            builder.Property(pm => pm.AssignedAt).IsRequired();
            builder.HasOne(pm => pm.Project).WithMany(p => p.Members).HasForeignKey(pm => pm.ProjectId);
        }
    }
}