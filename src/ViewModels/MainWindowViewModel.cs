using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Common;
using DataModel;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels.Modules.Competitions;
using ViewModels.Modules.Skaters;
using ViewModels.Modules.Teams;

namespace ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(CompetitionsViewModel competitionsViewModel, ISchedulerProvider schedulerProvider,
            Func<Competition, TeamsViewModel> teamsFactory, Func<Competition, SkatersViewModel> skatersFactory)
        {
            CompetitionsViewModel =
                competitionsViewModel ?? throw new ArgumentNullException(nameof(competitionsViewModel));

            CompetitionsDialog = new Interaction<CompetitionsViewModel, CompetitionProxy>(schedulerProvider.Dispatcher);

            InitializeProperties(teamsFactory, skatersFactory);

            SearchCompetitions = ReactiveCommand.CreateFromTask(ShowSearchDialogAsync);
        }

        public CompetitionsViewModel CompetitionsViewModel { get; }

        [ObservableAsProperty] public CompetitionExtendedProxy? CompetitionExtended { get; set; }

        [ObservableAsProperty] public bool CompetitionHasValue { get; } = false;

        public Interaction<CompetitionsViewModel, CompetitionProxy> CompetitionsDialog { get; }

        public ReactiveCommand<Unit, Unit> SearchCompetitions { get; }

        private void InitializeProperties(Func<Competition, TeamsViewModel> teamsFactory, Func<Competition, SkatersViewModel> skatersFactory)
        {
            this.WhenValueChanged(viewModel => viewModel.CompetitionsViewModel.SelectedCompetitionProxy)
                .Select(competitionProxy =>
                {
                    if (competitionProxy == null)
                    {
                        return null;
                    }

                    return new CompetitionExtendedProxy(
                        teamsFactory(competitionProxy),
                        skatersFactory(competitionProxy),
                        competitionProxy);
                })
                .ToPropertyEx(this, x => x.CompetitionExtended);

            this.WhenValueChanged(viewModel => viewModel.CompetitionExtended)
                .Select(competitionProxy => competitionProxy is not null)
                .ToPropertyEx(this, mainWindowViewModel => mainWindowViewModel.CompetitionHasValue);
        }

        private async Task ShowSearchDialogAsync()
        {
            await CompetitionsDialog.Handle(CompetitionsViewModel);
        }
    }
}
