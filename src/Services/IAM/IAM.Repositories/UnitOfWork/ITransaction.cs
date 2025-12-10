namespace IAM.Repositories.UnitOfWork
{
    public interface ITransaction : IAsyncDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
}
