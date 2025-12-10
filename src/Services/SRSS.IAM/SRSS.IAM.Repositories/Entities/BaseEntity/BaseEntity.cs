namespace SRSS.IAM.Repositories.Entities.BaseEntity
{
    public abstract class BaseEntity<T> : IBaseEntity<T>
    {
        public T Id { get; set; } = default!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
