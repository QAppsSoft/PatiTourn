using DataModel;

namespace Domain.Services.Interfaces
{
    public readonly record struct UpdateEntityContainer<TEntity>(TEntity EditedEntity, TEntity DatabaseEntity)
        where TEntity : BaseEntity;
}
