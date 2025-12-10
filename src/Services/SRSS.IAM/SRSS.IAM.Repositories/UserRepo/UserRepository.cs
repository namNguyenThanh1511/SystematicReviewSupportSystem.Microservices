using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.GenericRepository;

namespace SRSS.IAM.Repositories.UserRepo
{
    public class UserRepository : GenericRepository<User, Guid>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }
    }
}
