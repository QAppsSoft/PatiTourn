using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace ViewModels.Interfaces
{
    public interface IEntitiesProxyContainerViewModel<TProxy>
    {
        ReactiveCommand<Unit, int> Save { get; }

        ReactiveCommand<Unit, Unit> AddNew { get; }

        ReactiveCommand<Unit, Unit> Refresh { get; }

        ReactiveCommand<TProxy, Unit> Delete { get; }

        ReactiveCommand<TProxy, Unit> Edit { get; }

        TProxy? SelectedItem { get; set; }

        public ReadOnlyObservableCollection<TProxy> Items { get; }
    }
}
