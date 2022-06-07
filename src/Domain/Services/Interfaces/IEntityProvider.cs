using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataModel;
using DynamicData;

namespace Domain.Services.Interfaces
{
    public interface IEntityProvider<TEntity> : IDisposable
        where TEntity : BaseEntity
    {
        IObservableList<TEntity> List { get; }

        Task RefreshAsync();

        Task RefreshAsync(Expression<Func<TEntity, bool>> filter);
    }
}
