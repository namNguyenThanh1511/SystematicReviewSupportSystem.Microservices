using Shared.Repositories;
using SRSS.Project.Infrastructure.Data;

namespace SRSS.Project.Infrastructure.ProjectRepo
{
    public class ProjectRepository : GenericRepository<Domain.Entities.Project, Guid, ProjectDbContext>, IProjectRepository
    {
        public ProjectRepository(ProjectDbContext context) : base(context)
        {
        }
    }
}
