using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.Project.Domain.Entities;

namespace SRSS.Project.Infrastructure.Configurations
{
    public class SearchSourceConfiguration : IEntityTypeConfiguration<SearchSource>
    {
        public void Configure(EntityTypeBuilder<SearchSource> builder)
        {
            builder.ToTable("search_sources");

            builder.HasKey(ss => ss.Id);

            builder.Property(ss => ss.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(ss => ss.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();

            builder.Property(ss => ss.SourceName)
                .HasColumnName("source_name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(ss => ss.SourceType)
                .HasColumnName("source_type")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(ss => ss.PlannedSearchString)
                .HasColumnName("planned_search_string")
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(ss => ss.Notes)
                .HasColumnName("notes")
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(ss => ss.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(ss => ss.ModifiedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            // Relationships
            builder.HasOne(ss => ss.Project)
                .WithMany(p => p.SearchSources)
                .HasForeignKey(ss => ss.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(ss => ss.ProjectId);
        }
    }
}
