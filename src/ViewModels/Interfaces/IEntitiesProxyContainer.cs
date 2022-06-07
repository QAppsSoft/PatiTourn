using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace ViewModels.Interfaces
{
    public interface IEntitiesProxyContainer<TProxy>
    {
        ReactiveCommand<Unit, Unit> AddNew { get; }

        ReactiveCommand<Unit, Unit> Refresh { get; }

        ReactiveCommand<TProxy, Unit> Delete { get; }

        ReactiveCommand<TProxy, Unit> Edit { get; }

        TProxy? SelectedProxy { get; set; }

        public ReadOnlyObservableCollection<TProxy> ProxyItems { get; }
    }
}
