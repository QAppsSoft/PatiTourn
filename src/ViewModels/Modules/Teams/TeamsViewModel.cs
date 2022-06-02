using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Common;
using DataModel;
using Domain.Services.Interfaces;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace ViewModels.Modules.Teams
{
    public class TeamsViewModel : ValidatableViewModelBase, IDisposable
    {
        private readonly IEntityService<Team> _teamsService;
        private readonly IDisposable _cleanup;

        public TeamsViewModel(IEntityService<Team> teamsService, ISchedulerProvider schedulerProvider,
            Competition competition)
        {
            _teamsService = teamsService ?? throw new ArgumentNullException(nameof(teamsService));

            var transform = teamsService.List.Connect()
                .Transform(team => new TeamProxy(team))
                .Publish();

            var teamsListDisposable = transform
                .AutoRefresh()
                .Sort(SortExpressionComparer<TeamProxy>.Ascending(teamProxy => teamProxy.Name))
                .ObserveOn(schedulerProvider.Dispatcher)
                .Bind(out var teams)
                .Subscribe();

            Teams = teams;

            var onTeamsListChanged = transform.ObserveOn(schedulerProvider.Dispatcher)
                .ActOnEveryObject(_ => { }, OnRemove);

            var anySelected = this.WhenAnyValue(vm => vm.SelectedTeamProxy)
                .Select(teamProxy => teamProxy != null)
                .Publish();

            EditDialog = new Interaction<TeamProxy, Unit>(schedulerProvider.Dispatcher);

            ConfirmDeleteDialog = new Interaction<TeamProxy, bool>(schedulerProvider.Dispatcher);
            
            Delete = ReactiveCommand.CreateFromTask<TeamProxy>(DeleteTeamAsync, anySelected);

            Refresh = ReactiveCommand.Create(() => teamsService.Refresh(team => team.CompetitionId == competition.Id));

            AddNew = ReactiveCommand.Create(() =>
            {
                var team = teamsService.Create();
                team.CompetitionId = competition.Id;

                teamsService.Add(team);
            });

            var onCompetitionAdded = transform
                .SkipUntil(Refresh) // Wait until existing data is loaded
                .ObserveOn(schedulerProvider.Dispatcher)
                .ActOnEveryObject(competitionProxy => EditNewTeamAsync(competitionProxy).SafeFireAndForget(), _ => { });

            Edit = ReactiveCommand.CreateFromTask<TeamProxy>(EditTeamAsync, anySelected);

            var allValid = transform.AutoRefreshOnObservable(teamProxy => teamProxy.IsValid())
                .Filter(skaterProxy => !skaterProxy.ValidationContext.IsValid)
                .Count()
                .Select(count => count == 0)
                .StartWith(true)
                .Publish();

            Save = ReactiveCommand.CreateFromTask(teamsService.SaveAsync, allValid);

            this.ValidationRule(viewModel => viewModel.Teams, allValid, "A Team info is in an invalid state");

            _cleanup = new CompositeDisposable(teamsListDisposable, onTeamsListChanged, onCompetitionAdded, transform.Connect(), allValid.Connect(), anySelected.Connect());
        }

        private async Task DeleteTeamAsync(TeamProxy team)
        {
            var canDelete = await ConfirmDeleteDialog.Handle(team);

            if (canDelete)
            {
                _teamsService.Remove(team);
                await _teamsService.SaveAsync().ConfigureAwait(false);
            }
        }

        private async Task EditNewTeamAsync(TeamProxy teamProxy)
        {
            await EditTeamAsync(teamProxy).ConfigureAwait(true);
            SelectedTeamProxy = teamProxy;
        }
        private async Task EditTeamAsync(TeamProxy teamProxy)
        {
            await EditDialog.Handle(teamProxy);
            await _teamsService.SaveAsync().ConfigureAwait(false);
        }

        private void OnRemove(TeamProxy teamProxy)
        {
            if (SelectedTeamProxy == teamProxy)
            {
                SelectedTeamProxy = null;
            }
        }

        public Interaction<TeamProxy, Unit> EditDialog { get; }

        public Interaction<TeamProxy, bool> ConfirmDeleteDialog { get; }

        public ReactiveCommand<Unit, int> Save { get; }

        public ReactiveCommand<Unit, Unit> AddNew { get; }

        public ReactiveCommand<TeamProxy, Unit> Edit { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        public ReactiveCommand<TeamProxy, Unit> Delete { get; }

        public ReadOnlyObservableCollection<TeamProxy> Teams { get; }

        [Reactive] public TeamProxy? SelectedTeamProxy { get; set; }

        public void Dispose()
        {
            _cleanup.Dispose();
        }
    }
}
