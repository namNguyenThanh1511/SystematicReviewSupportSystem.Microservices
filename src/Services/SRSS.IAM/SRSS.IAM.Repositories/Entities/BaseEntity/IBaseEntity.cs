namespace SRSS.IAM.Repositories.Entities.BaseEntity
{
    public interface IBaseEntity
    {
        DateTime? CreatedAt { get; set; }
        DateTime? ModifiedAt { get; set; }
    }

    public interface IBaseEntity<T> : IBaseEntity
    {
        T Id { get; set; }
    }
}
