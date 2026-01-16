using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SRSS.Project.Infrastructure.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Domain.Entities.Project>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Project> builder)
        {
            builder.ToTable("projects");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(p => p.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(p => p.Abbreviation)
                .HasColumnName("abbreviation")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasColumnName("description")
                .HasColumnType("text");

            builder.Property(p => p.ResearchQuestions)
                .HasColumnName("research_questions")
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(p => p.InclusionCriteria)
                .HasColumnName("inclusion_criteria")
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(p => p.ExclusionCriteria)
                .HasColumnName("exclusion_criteria")
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(p => p.Phase)
                .HasColumnName("phase")
                .HasConversion<string>()
                .IsRequired();

            //builder.Property(p => p.CriteriaVersion)
            //    .HasColumnName("criteria_version")
            //    .HasDefaultValue(1)
            //    .IsRequired();

            builder.Property(p => p.PhaseChangedAt)
                .HasColumnName("phase_changed_at");

            builder.Property(p => p.StartDate)
                .HasColumnName("start_date");

            builder.Property(p => p.ExpectedEndDate)
                .HasColumnName("expected_end_date");

            builder.Property(p => p.ActualEndDate)
                .HasColumnName("actual_end_date");

            builder.Property(p => p.CreatedBy)
                .HasColumnName("created_by");

            builder.Property(p => p.UpdatedBy)
                .HasColumnName("updated_by");

            builder.Property(p => p.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(p => p.ModifiedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            // Relationships
            builder.HasMany(p => p.Members)
                .WithOne(m => m.Project)
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.SearchSources)
                .WithOne(s => s.Project)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
