using Shared.Repositories;
using SRSS.Project.Infrastructure.Data;
using SRSS.Project.Infrastructure.ProjectRepo;

namespace SRSS.Project.Infrastructure.Repositories
{
    public interface IUnitOfWork : IUnitOfWork<ProjectDbContext>
    {
        public IProjectRepository Projects { get; }
    }
}
