using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.GenericRepository;

namespace SRSS.IAM.Repositories.UserRepo
{
    public interface IUserRepository : IGenericRepository<User, Guid>
    {
    }
}
