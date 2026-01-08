using Microsoft.EntityFrameworkCore;

namespace SRSS.Project.Infrastructure.Data
{
    public class ProjectDbContext(DbContextOptions<ProjectDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
               => builder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }


        public DbSet<Domain.Entities.Project> Projects { get; set; }
        public DbSet<Domain.Entities.ProjectMember> ProjectMembers { get; set; }
        public DbSet<Domain.Entities.ProjectStageStage> ProjectStages { get; set; }
    }
}
