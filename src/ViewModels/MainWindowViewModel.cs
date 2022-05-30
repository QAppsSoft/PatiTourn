﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels.Modules.Competitions;

namespace ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(CompetitionsViewModel competitionsViewModel)
        {
            CompetitionsViewModel =
                competitionsViewModel ?? throw new ArgumentNullException(nameof(competitionsViewModel));

            SearchCompetitions = ReactiveCommand.CreateFromTask(ShowSearchDialogAsync);

            var _ = this.WhenValueChanged(mainWindowViewModel =>
                    mainWindowViewModel.CompetitionsViewModel.SelectedCompetitionProxy)
                .Select(competitionProxy => competitionProxy is not null)
                .ToPropertyEx(this, mainWindowViewModel => mainWindowViewModel.CompetitionHasValue);
        }

        public CompetitionsViewModel CompetitionsViewModel { get; }

        [ObservableAsProperty] public bool CompetitionHasValue { get; } = false;

        public Interaction<CompetitionsViewModel, CompetitionProxy> CompetitionsDialog { get; } = new();

        public ReactiveCommand<Unit, Unit> SearchCompetitions { get; }

        private async Task ShowSearchDialogAsync()
        {
            await CompetitionsDialog.Handle(CompetitionsViewModel);
        }
    }
}
