using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Common;
using DataModel;
using Domain.Services.Interfaces;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ViewModels.Interfaces;

namespace ViewModels.Modules.Skaters
{
    public class SkatersViewModel : ValidatableViewModelBase, IEntitiesProxyContainerViewModel<SkaterProxy>, IDisposable
    {
        private readonly IEntityService<Skater> _skatersService;
        private readonly Competition _competition;
        private readonly IDisposable _cleanup;

        public SkatersViewModel(IEntityService<Skater> skatersService, ISchedulerProvider schedulerProvider, Competition competition)
        {
            _skatersService = skatersService ?? throw new ArgumentNullException(nameof(skatersService));
            _competition = competition ?? throw new ArgumentNullException(nameof(competition));

            var transform = skatersService.List.Connect()
                .Transform(skater => new SkaterProxy(skater))
                .Publish();

            var skatersListDisposable = transform
                .Sort(SortExpressionComparer<SkaterProxy>.Ascending(x => x.Team.Id).ThenByAscending(x => x.Number))
                .ObserveOn(schedulerProvider.Dispatcher)
                .Bind(out var skaters)
                .Subscribe();

            ProxyItems = skaters;

            EditDialog = new Interaction<SkaterProxy, Unit>(schedulerProvider.Dispatcher);

            ConfirmDeleteDialog = new Interaction<SkaterProxy, bool>(schedulerProvider.Dispatcher);

            AddDialog = new Interaction<SkaterProxy, bool>(schedulerProvider.Dispatcher);

            var anySelected = this.WhenAnyValue(vm => vm.SelectedProxy)
                .Select(skaterProxy => skaterProxy != null)
                .Publish();

            Delete = ReactiveCommand.CreateFromTask<SkaterProxy>(DeleteSkaterAsync, anySelected);

            Refresh = ReactiveCommand.Create(() => skatersService.Refresh(skater => skater.CompetitionId == competition.Id));

            AddNew = ReactiveCommand.CreateFromTask(AddSkaterAsync);

            Edit = ReactiveCommand.CreateFromTask<SkaterProxy>(EditSkaterAsync, anySelected);

            var allValid = transform.AutoRefreshOnObservable(skaterProxy => skaterProxy.IsValid())
                .Filter(skaterProxy => !skaterProxy.ValidationContext.IsValid)
                .Count()
                .Select(count => count == 0)
                .StartWith(true)
                .Publish();

            Save = ReactiveCommand.CreateFromTask(skatersService.SaveAsync, allValid);

            this.ValidationRule(viewModel => viewModel.ProxyItems, allValid, "A Skater info is in an invalid state");

            _cleanup = new CompositeDisposable(skatersListDisposable, transform.Connect(), allValid.Connect(), anySelected.Connect());
        }

        private async Task EditSkaterAsync(SkaterProxy skaterProxy)
        {
            await EditDialog.Handle(skaterProxy);
            await _skatersService.SaveAsync().ConfigureAwait(false);
        }

        private async Task DeleteSkaterAsync(SkaterProxy skaterProxy)
        {
            var canDelete = await ConfirmDeleteDialog.Handle(skaterProxy);

            if (canDelete)
            {
                _skatersService.Remove(skaterProxy);
                await _skatersService.SaveAsync().ConfigureAwait(false);
            }
        }

        private async Task AddSkaterAsync()
        {
            var skater = _skatersService.Create();
            skater.CompetitionId = _competition.Id;

            var result = await AddDialog.Handle(new SkaterProxy(skater));
            
            if (result)
            {
                _skatersService.Add(skater);
            }
        }

        public Interaction<SkaterProxy, Unit> EditDialog { get; }

        public Interaction<SkaterProxy, bool> ConfirmDeleteDialog { get; }

        public Interaction<SkaterProxy, bool> AddDialog { get; }

        public ReactiveCommand<Unit, Unit> AddNew { get; }

        public ReactiveCommand<SkaterProxy, Unit> Edit { get; }

        public ReactiveCommand<SkaterProxy, Unit> Delete { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        public ReactiveCommand<Unit, int> Save { get; }

        public ReadOnlyObservableCollection<SkaterProxy> ProxyItems { get; }

        [Reactive] public SkaterProxy? SelectedProxy { get; set; }

        public void Dispose()
        {
            _cleanup.Dispose();
            AddNew.Dispose();
            Edit.Dispose();
            Delete.Dispose();
            Refresh.Dispose();
        }
    }
}
