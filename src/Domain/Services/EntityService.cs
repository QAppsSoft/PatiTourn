using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataModel;
using Domain.Services.Interfaces;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Domain.Services
{
    public class EntityService<TEntity> : IEntityService<TEntity>
        where TEntity : BaseEntity, new()
    {
        private readonly PatiTournDataBaseContext _dataBaseContext;
        private readonly SourceList<TEntity> _entityList;

        public EntityService(PatiTournDataBaseContext dataBaseContext)
        {
            _dataBaseContext = dataBaseContext ?? throw new ArgumentNullException(nameof(dataBaseContext));

            var source = _dataBaseContext.Set<TEntity>()
                .Local
                //.ToObservableChangeSet<LocalView<TEntity>, TEntity>(); // TODO: Check why this is failing
                .ToObservableCollection()
                .ToObservableChangeSet();

            _entityList = new SourceList<TEntity>(source);

            List = _entityList.AsObservableList();
        }

        public IObservableList<TEntity> List { get; }

        public void Add(TEntity skater)
        {
            _dataBaseContext.Add(skater);
        }

        public void Remove(TEntity skater)
        {
            _dataBaseContext.Remove(skater);
        }

        public TEntity Create()
        {
            return new TEntity { Id = Guid.NewGuid() };
        }

        public void Refresh()
        {
            _dataBaseContext.Set<TEntity>().Load();
        }

        public void Refresh(Expression<Func<TEntity, bool>> filter)
        {
            _dataBaseContext.Set<TEntity>().Where(filter).Load();
        }

        public bool IsDirty(TEntity skater)
        {
            var state = _dataBaseContext.Entry(skater).State;

            return state switch
            {
                EntityState.Modified => true,
                EntityState.Added => true,
                EntityState.Detached => false,
                EntityState.Unchanged => false,
                EntityState.Deleted => false,
                _ => false
            };
        }

        public Task<int> SaveAsync()
        {
            return _dataBaseContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dataBaseContext.Dispose();
            _entityList.Dispose();
            List.Dispose();
        }
    }
}
