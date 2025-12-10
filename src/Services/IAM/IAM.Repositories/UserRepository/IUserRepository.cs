

using IAM.Repositories.Entities;
using IAM.Repositories.GenericRepository;

namespace IAM.Repositories.UserRepository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByPhoneNumberAsync(string phone);
    }
}
