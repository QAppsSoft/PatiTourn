using System;
using System.Threading.Tasks;
using DataModel;

namespace Domain.Services.Interfaces
{
    public interface IEntityManager<TEntity> : IDisposable
        where TEntity : BaseEntity
    {
        void Add(TEntity skater);

        void Remove(TEntity skater);

        TEntity Create();

        bool IsDirty(TEntity skater);

        Task<int> SaveAsync();
    }
}
