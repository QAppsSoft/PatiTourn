using DataModel;

namespace Domain.Services.Interfaces
{
    public interface IEntityService<TEntity> : IEntityProvider<TEntity>, IEntityManager<TEntity>
        where TEntity : BaseEntity
    {
    }
}
