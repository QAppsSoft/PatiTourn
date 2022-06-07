using System;
using System.Threading.Tasks;
using DataModel;

namespace Domain.Services.Interfaces
{
    public interface IEntityManager<TEntity> : IDisposable
        where TEntity : BaseEntity
    {
        Task AddAsync(TEntity skater);

        Task RemoveAsync(TEntity skater);

        Task EditAsync(TEntity updatedEntity, Action<UpdateEntityContainer<TEntity>> updateProperties);

        TEntity Create();
    }
}
