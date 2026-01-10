using SRSS.Project.Infrastructure.ProjectRepo;

namespace SRSS.Project.Infrastructure.Repositories
{
    public class UnitOfWork : Shared.Repositories.UnitOfWork<Data.ProjectDbContext>, IUnitOfWork
    {
        // Define repositories here
        // private IYourRepository _yourRepository;
        private IProjectRepository _projects;
        public UnitOfWork(Data.ProjectDbContext dbContext) : base(dbContext)
        {
            // _dbContext is managed by the base class
        }
        // Example of a repository property
        // public IYourRepository YourRepository
        //     => _yourRepository ??= new YourRepository(_dbContext);
        public IProjectRepository Projects
            => _projects ??= new ProjectRepository(_dbContext);
    }

}
