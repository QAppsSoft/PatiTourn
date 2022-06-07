using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataModel;
using Domain.Services.Interfaces;
using DynamicData;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services
{
    public class EntityService<TEntity> : IEntityService<TEntity>
        where TEntity : BaseEntity, new()
    {
        private readonly Func<DbContext> _dataBaseContextFactory;
        private readonly SourceList<TEntity> _entityList;

        public EntityService(Func<DbContext> dataBaseContextFactory)
        {
            ArgumentNullException.ThrowIfNull(dataBaseContextFactory);

            _dataBaseContextFactory = dataBaseContextFactory;
            
            _entityList = new SourceList<TEntity>();

            List = _entityList.AsObservableList();
        }

        public IObservableList<TEntity> List { get; }

        public async Task AddAsync(TEntity skater)
        {
            ArgumentNullException.ThrowIfNull(skater);

            var context = _dataBaseContextFactory();
            await using var _ = context.ConfigureAwait(false);

            context.Add(skater);
            await context.SaveChangesAsync().ConfigureAwait(false);

            _entityList.Add(skater);
        }

        public async Task RemoveAsync(TEntity skater)
        {
            ArgumentNullException.ThrowIfNull(skater);

            var context = _dataBaseContextFactory();
            await using var _ = context.ConfigureAwait(false);
            
            context.Remove(skater);
            await context.SaveChangesAsync().ConfigureAwait(false);

            _entityList.Remove(skater);
        }

        public async Task EditAsync(TEntity updatedEntity, Action<UpdateEntityContainer<TEntity>> updateProperties)
        {
            ArgumentNullException.ThrowIfNull(updatedEntity);
            ArgumentNullException.ThrowIfNull(updateProperties);

            var context = _dataBaseContextFactory();
            await using var _ = context.ConfigureAwait(false);

            var dbEntity = await context.Set<TEntity>().FindAsync(updatedEntity.Id).ConfigureAwait(false);

            if (dbEntity == null)
            {
                _entityList.Remove(updatedEntity);
            }
            else
            {
                updateProperties(new UpdateEntityContainer<TEntity>(updatedEntity, dbEntity));
            }

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public TEntity Create()
        {
            return new TEntity { Id = Guid.NewGuid() };
        }

        public async Task RefreshAsync()
        {
            var context = _dataBaseContextFactory();
            await using var _ = context.ConfigureAwait(false);

            var entities = await context.Set<TEntity>().ToListAsync().ConfigureAwait(false);
            
            _entityList.Edit(cache =>
            {
                cache.Clear();
                cache.AddRange(entities);
            });
        }

        public async Task RefreshAsync(Expression<Func<TEntity, bool>> filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var context = _dataBaseContextFactory();
            await using var _ = context.ConfigureAwait(false);

            var entities = await context.Set<TEntity>().Where(filter).ToListAsync().ConfigureAwait(false);

            _entityList.Edit(cache =>
            {
                cache.Clear();
                cache.AddRange(entities);
            });
        }
        
        public void Dispose()
        {
            _entityList.Dispose();
            List.Dispose();
        }
    }
}
