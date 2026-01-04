using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.Project.Domain.Entities;

namespace SRSS.Project.Infrastructure.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Domain.Entities.Project>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Project> builder)
        {
            builder.ToTable("Projects");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.NameEn).HasMaxLength(255).IsRequired();
            builder.Property(p => p.NameVn).HasMaxLength(255).IsRequired();
            builder.Property(p => p.Abbreviation).HasMaxLength(255).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(1000).IsRequired();
            builder.Property(p => p.ResearchQuestions).HasMaxLength(1000).IsRequired();
            builder.Property(p => p.ProsperoId).HasMaxLength(255).IsRequired();
            builder.Property(p => p.InclusionCriteria).HasMaxLength(1000).IsRequired();
            builder.Property(p => p.ExclusionCriteria).HasMaxLength(1000).IsRequired();
            builder.Property(p => p.StartDate).IsRequired();
            builder.Property(p => p.EndDate).IsRequired();
            builder.Property(p => p.CreatedBy).IsRequired();
            builder.Property(p => p.Status).IsRequired();
            builder.Property(p => p.Status).HasConversion<string>();
            builder.Property(p => p.RowVersion).IsRequired();
            builder.HasMany(p => p.Members).WithOne(m => m.Project).HasForeignKey(m => m.ProjectId);
            builder.HasMany(p => p.Stages).WithOne(s => s.Project).HasForeignKey(s => s.ProjectId);
        }
    }
}
