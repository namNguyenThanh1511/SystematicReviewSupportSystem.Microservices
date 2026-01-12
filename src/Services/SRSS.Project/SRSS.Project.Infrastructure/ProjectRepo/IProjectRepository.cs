using Shared.Repositories;
using SRSS.Project.Infrastructure.Data;

namespace SRSS.Project.Infrastructure.ProjectRepo
{
    public interface IProjectRepository : IGenericRepository<Domain.Entities.Project, Guid, ProjectDbContext>
    {
    }
}
