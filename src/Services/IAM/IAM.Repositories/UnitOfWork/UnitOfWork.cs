using IAM.Repositories.UserRepository;
using Microsoft.EntityFrameworkCore.Storage;


namespace IAM.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IAMDbContext _context;

        private IDbContextTransaction? _transaction;



        public IUserRepository Users { get; private set; }



        public UnitOfWork(IAMDbContext context, IUserRepository userRepository)
        {
            _context = context;
            Users = userRepository;
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task BeginTransaction()
        {
            if (_context.Database.CurrentTransaction == null)
            {
                await _context.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitTransaction()
        {
            try
            {
                await _context.Database.CommitTransactionAsync();
                _transaction?.Commit();

            }
            catch
            {
                await RollbackTransaction();
                throw;
            }
            finally
            {
                if (_context.Database.CurrentTransaction != null)
                {
                    await _context.Database.CurrentTransaction.DisposeAsync();
                }
            }

        }

        public async Task RollbackTransaction()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        public async Task<ITransaction?> GetCurrentTransaction()
        {
            throw new NotImplementedException();

        }
    }
}
