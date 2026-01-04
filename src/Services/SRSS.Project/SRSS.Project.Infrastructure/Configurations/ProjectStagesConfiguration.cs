using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SRSS.Project.Infrastructure.Configurations
{
    public class ProjectStagesConfiguration : IEntityTypeConfiguration<Domain.Entities.ProjectStageStage>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.ProjectStageStage> builder)
        {
            builder.ToTable("ProjectStages");
            builder.HasKey(ps => ps.Id);
            builder.Property(ps => ps.ProjectId).IsRequired();
            builder.Property(ps => ps.StageName).IsRequired();
            builder.Property(ps => ps.StageName).HasConversion<string>();
            builder.Property(ps => ps.Status).IsRequired();
            builder.Property(ps => ps.Status).HasConversion<string>();
            builder.Property(ps => ps.CompletionPercentage).IsRequired();
            builder.HasOne(ps => ps.Project).WithMany(p => p.Stages).HasForeignKey(ps => ps.ProjectId);
        }
    }
}