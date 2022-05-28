using System;
using System.Linq.Expressions;
using DataModel;
using DynamicData;

namespace Domain.Services.Interfaces
{
    public interface IEntityProvider<TEntity> : IDisposable
        where TEntity : BaseEntity
    {
        IObservableList<TEntity> List { get; }

        void Refresh();

        void Refresh(Expression<Func<TEntity, bool>> filter);
    }
}
