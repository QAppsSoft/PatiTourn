using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
        private readonly IDisposable _cleanup;

        public TeamsViewModel(IEntityService<Team> teamsService, ISchedulerProvider schedulerProvider,
            Competition competition)
        {
            var transform = teamsService.List.Connect()
                .Transform(team => new TeamProxy(team))
                .Publish();

            var teamsListDisposable = transform
                .Sort(SortExpressionComparer<TeamProxy>.Ascending(teamProxy => teamProxy.Name))
                .ObserveOn(schedulerProvider.Dispatcher)
                .Bind(out var teams)
                .Subscribe();

            Teams = teams;

            var selectLast = transform.ActOnEveryObject(teamProxy => SelectedTeamProxy = teamProxy, _ => { });

            var anySelected = this.WhenAnyValue(vm => vm.SelectedTeamProxy)
                .Select(teamProxy => teamProxy != null)
                .Publish();

            Delete = ReactiveCommand.Create<Team>(teamsService.Remove, anySelected);

            Refresh = ReactiveCommand.Create(() => teamsService.Refresh(team => team.CompetitionId == competition.Id));

            AddNew = ReactiveCommand.Create(() =>
            {
                var team = teamsService.Create();
                team.Competition = competition;

                teamsService.Add(team);
            });

            var allValid = transform.AutoRefreshOnObservable(teamProxy => teamProxy.IsValid())
                .Filter(skaterProxy => !skaterProxy.ValidationContext.IsValid)
                .Count()
                .Select(count => count == 0)
                .StartWith(true)
                .Publish();

            Save = ReactiveCommand.CreateFromTask(teamsService.SaveAsync, allValid);

            this.ValidationRule(viewModel => viewModel.Teams, allValid, "A Team info is in an invalid state");

            _cleanup = new CompositeDisposable(teamsListDisposable, selectLast, transform.Connect(), allValid.Connect(), anySelected.Connect());
        }

        public ReactiveCommand<Unit, int> Save { get; }

        public ReactiveCommand<Unit, Unit> AddNew { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        public ReactiveCommand<Team, Unit> Delete { get; }

        public ReadOnlyObservableCollection<TeamProxy> Teams { get; }

        [Reactive] public TeamProxy? SelectedTeamProxy { get; set; }

        public void Dispose()
        {
            _cleanup.Dispose();
        }
    }
}
