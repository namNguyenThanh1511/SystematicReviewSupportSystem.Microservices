using IAM.Repositories.UserRepository;

namespace IAM.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        Task BeginTransaction();

        Task CommitTransaction();

        Task RollbackTransaction();

        Task<ITransaction?> GetCurrentTransaction();

        Task<int> SaveChangesAsync();
    }
}
