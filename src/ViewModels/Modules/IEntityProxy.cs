using DataModel;

namespace ViewModels.Modules
{
    public interface IEntityProxy<out TBaseEntity>
        where TBaseEntity : BaseEntity
    {
        TBaseEntity Entity { get; }
    }
}
