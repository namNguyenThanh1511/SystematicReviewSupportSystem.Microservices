using IAM.Repositories.Entities;
using IAM.Repositories.GenericRepository;

namespace IAM.Repositories.UserRepository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(IAMDbContext context) : base(context)
        {

        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByPhoneNumberAsync(string phone)
        {
            throw new NotImplementedException();
        }
    }
}
