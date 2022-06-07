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
    public class SkatersViewModel : ValidatableViewModelBase, IEntitiesProxyContainer<SkaterProxy>, IDisposable
    {
        private readonly IEntityService<Skater> _skatersService;
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly IEntityProvider<Team> _teamsProvider;
        private readonly Competition _competition;
        private readonly IDisposable _cleanup;

        public SkatersViewModel(IEntityService<Skater> skatersService, ISchedulerProvider schedulerProvider, IEntityProvider<Team> teamsProvider, Competition competition)
        {
            ArgumentNullException.ThrowIfNull(skatersService);
            ArgumentNullException.ThrowIfNull(teamsProvider);
            ArgumentNullException.ThrowIfNull(schedulerProvider);
            ArgumentNullException.ThrowIfNull(competition);

            _skatersService = skatersService;
            _schedulerProvider = schedulerProvider;
            _teamsProvider = teamsProvider;
            _competition = competition;

            var transform = skatersService.List.Connect()
                .Transform(skater => new SkaterProxy(skater, teamsProvider, schedulerProvider))
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

            Refresh = ReactiveCommand.CreateFromTask(() => RefreshAsync(skatersService, competition));

            AddNew = ReactiveCommand.CreateFromTask(AddSkaterAsync);

            Edit = ReactiveCommand.CreateFromTask<SkaterProxy>(EditSkaterAsync, anySelected);

            var allValid = transform.AutoRefreshOnObservable(skaterProxy => skaterProxy.IsValid())
                .Filter(skaterProxy => !skaterProxy.ValidationContext.IsValid)
                .Count()
                .Select(count => count == 0)
                .StartWith(true)
                .Publish();

            this.ValidationRule(viewModel => viewModel.ProxyItems, allValid, "A Skater info is in an invalid state");

            _cleanup = new CompositeDisposable(skatersListDisposable, transform.Connect(), allValid.Connect(), anySelected.Connect());
        }

        private async Task RefreshAsync(IEntityProvider<Skater> skatersService, BaseEntity competition)
        {
            await _teamsProvider.RefreshAsync(team => team.CompetitionId == competition.Id).ConfigureAwait(false);
            await skatersService.RefreshAsync(skater => skater.CompetitionId == competition.Id).ConfigureAwait(false);
        }

        private async Task EditSkaterAsync(SkaterProxy skaterProxy)
        {
            await _teamsProvider.RefreshAsync(team => team.CompetitionId == _competition.Id).ConfigureAwait(false);

            await EditDialog.Handle(skaterProxy);
            await _skatersService.EditAsync(skaterProxy, UpdateDataBaseEntityProperties).ConfigureAwait(false);
        }

        private static void UpdateDataBaseEntityProperties(UpdateEntityContainer<Skater> container)
        {
            var (editedEntity, databaseEntity) = container;
            databaseEntity.Name = editedEntity.Name;
            databaseEntity.LastNames = editedEntity.LastNames;
            databaseEntity.IdentificationNumber = editedEntity.IdentificationNumber;
            databaseEntity.Sex = editedEntity.Sex;
            databaseEntity.TeamId = editedEntity.TeamId;
            databaseEntity.CompetitionId = editedEntity.CompetitionId;
        }

        private async Task DeleteSkaterAsync(SkaterProxy skaterProxy)
        {
            var canDelete = await ConfirmDeleteDialog.Handle(skaterProxy);

            if (canDelete)
            {
                await _skatersService.RemoveAsync(skaterProxy).ConfigureAwait(false);
            }
        }

        private async Task AddSkaterAsync()
        {
            var skater = _skatersService.Create();
            skater.CompetitionId = _competition.Id;

            await _teamsProvider.RefreshAsync(team => team.CompetitionId == _competition.Id).ConfigureAwait(false);

            var result = await AddDialog.Handle(new SkaterProxy(skater, _teamsProvider, _schedulerProvider));

            if (result)
            {
                await _skatersService.AddAsync(skater).ConfigureAwait(false);
            }
        }

        public Interaction<SkaterProxy, Unit> EditDialog { get; }

        public Interaction<SkaterProxy, bool> ConfirmDeleteDialog { get; }

        public Interaction<SkaterProxy, bool> AddDialog { get; }

        public ReactiveCommand<Unit, Unit> AddNew { get; }

        public ReactiveCommand<SkaterProxy, Unit> Edit { get; }

        public ReactiveCommand<SkaterProxy, Unit> Delete { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

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
