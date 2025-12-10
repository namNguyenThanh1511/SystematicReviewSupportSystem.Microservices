using IAM.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAM.Repositories
{
    public class IAMDbContext : DbContext
    {
        public IAMDbContext(DbContextOptions<IAMDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        // Define DbSets for your entities here
        public DbSet<User> Users { get; set; }


    }
}
