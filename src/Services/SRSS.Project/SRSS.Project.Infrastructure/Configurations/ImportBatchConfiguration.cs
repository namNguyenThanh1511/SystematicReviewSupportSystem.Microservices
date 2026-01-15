using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.Project.Domain.Entities;

namespace SRSS.Project.Infrastructure.Configurations
{
    public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
    {
        public void Configure(EntityTypeBuilder<ImportBatch> builder)
        {
            builder.ToTable("import_batches");

            builder.HasKey(ib => ib.Id);

            builder.Property(ib => ib.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(ib => ib.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();

            builder.Property(ib => ib.SearchSourceId)
                .HasColumnName("search_source_id")
                .IsRequired();

            builder.Property(ib => ib.ExecutedAt)
                .HasColumnName("executed_at")
                .IsRequired();

            builder.Property(ib => ib.ExecutedSearchString)
                .HasColumnName("executed_search_string")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(ib => ib.RecordsRetrieved)
                .HasColumnName("records_retrieved")
                .IsRequired();

            builder.Property(ib => ib.Reason)
                .HasColumnName("reason")
                .HasMaxLength(255);

            builder.Property(ib => ib.PhaseAtImport)
                .HasColumnName("phase_at_import")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(ib => ib.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(ib => ib.ModifiedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            // Relationships
            builder.HasOne(ib => ib.Project)
                .WithMany()
                .HasForeignKey(ib => ib.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ib => ib.SearchSource)
                .WithMany()
                .HasForeignKey(ib => ib.SearchSourceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for better query performance
            builder.HasIndex(ib => ib.ProjectId);
            builder.HasIndex(ib => ib.SearchSourceId);
        }
    }
}
