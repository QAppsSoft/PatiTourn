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
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ViewModels.Interfaces;

namespace ViewModels.Modules.Teams
{
    public class TeamsViewModel : ValidatableViewModelBase, IEntitiesProxyContainer<TeamProxy>, IDisposable
    {
        private readonly IEntityService<Team> _teamsService;
        private readonly Competition _competition;
        private readonly IDisposable _cleanup;

        public TeamsViewModel(IEntityService<Team> teamsService, ISchedulerProvider schedulerProvider,
            Competition competition)
        {
            _teamsService = teamsService ?? throw new ArgumentNullException(nameof(teamsService));
            _competition = competition ?? throw new ArgumentNullException(nameof(competition));

            var transform = teamsService.List.Connect()
                .Transform(team => new TeamProxy(team))
                .Publish();

            var teamsListDisposable = transform
                .AutoRefresh()
                .Sort(SortExpressionComparer<TeamProxy>.Ascending(teamProxy => teamProxy.Name))
                .ObserveOn(schedulerProvider.Dispatcher)
                .Bind(out var teams)
                .Subscribe();

            ProxyItems = teams;

            var anySelected = this.WhenAnyValue(vm => vm.SelectedProxy)
                .Select(teamProxy => teamProxy != null)
                .Publish();

            EditDialog = new Interaction<TeamProxy, Unit>(schedulerProvider.Dispatcher);

            ConfirmDeleteDialog = new Interaction<TeamProxy, bool>(schedulerProvider.Dispatcher);

            AddDialog = new Interaction<TeamProxy, bool>(schedulerProvider.Dispatcher);
            
            Delete = ReactiveCommand.CreateFromTask<TeamProxy>(DeleteTeamAsync, anySelected);

            Refresh = ReactiveCommand.CreateFromTask(() => teamsService.RefreshAsync(team => team.CompetitionId == competition.Id));

            AddNew = ReactiveCommand.CreateFromTask(AddTeamAsync);

            Edit = ReactiveCommand.CreateFromTask<TeamProxy>(EditTeamAsync, anySelected);

            var allValid = transform.AutoRefreshOnObservable(teamProxy => teamProxy.IsValid())
                .Filter(skaterProxy => !skaterProxy.ValidationContext.IsValid)
                .Count()
                .Select(count => count == 0)
                .StartWith(true)
                .Publish();

            this.ValidationRule(viewModel => viewModel.ProxyItems, allValid, "A Team info is in an invalid state");

            _cleanup = new CompositeDisposable(teamsListDisposable, transform.Connect(), allValid.Connect(), anySelected.Connect());
        }

        private async Task AddTeamAsync()
        {
            var team = _teamsService.Create();
            team.CompetitionId = _competition.Id;

            var result = await AddDialog.Handle(new TeamProxy(team));

            if (result)
            {
                await _teamsService.AddAsync(team).ConfigureAwait(false);
            }
        }

        private async Task DeleteTeamAsync(TeamProxy team)
        {
            var canDelete = await ConfirmDeleteDialog.Handle(team);

            if (canDelete)
            {
                await _teamsService.RemoveAsync(team).ConfigureAwait(false);
            }
        }

        private async Task EditTeamAsync(TeamProxy teamProxy)
        {
            await EditDialog.Handle(teamProxy);



            await _teamsService.EditAsync(teamProxy, UpdateDataBaseEntityProperties).ConfigureAwait(false);
        }

        private static void UpdateDataBaseEntityProperties(UpdateEntityContainer<Team> container)
        {
            var (editedEntity, databaseEntity) = container;
            databaseEntity.Name = editedEntity.Name;
            databaseEntity.Description = editedEntity.Description;
            databaseEntity.CompetitionId = editedEntity.CompetitionId;
        }

        public Interaction<TeamProxy, Unit> EditDialog { get; }

        public Interaction<TeamProxy, bool> ConfirmDeleteDialog { get; }

        public Interaction<TeamProxy, bool> AddDialog { get; }

        public ReactiveCommand<Unit, Unit> AddNew { get; }

        public ReactiveCommand<TeamProxy, Unit> Edit { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        public ReactiveCommand<TeamProxy, Unit> Delete { get; }

        public ReadOnlyObservableCollection<TeamProxy> ProxyItems { get; }

        [Reactive] public TeamProxy? SelectedProxy { get; set; }

        public void Dispose()
        {
            _cleanup.Dispose();
        }
    }
}
