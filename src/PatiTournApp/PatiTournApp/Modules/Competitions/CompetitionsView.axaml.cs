using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ViewModels.Dialogs;
using ViewModels.Modules.Competitions;

namespace PatiTournApp.Modules.Competitions
{
    public partial class CompetitionsView : ReactiveUserControl<CompetitionsViewModel>
    {
        public CompetitionsView()
        {
            InitializeComponent();

            this.BindInteraction(ViewModel,
                vm => vm.ConfirmDeleteDialog,
                DoShowConfirmDeleteDialogAsync);

            this.BindInteraction(ViewModel,
                vm => vm.EditDialog,
                DoShowEditDialogAsync);

            this.BindInteraction(ViewModel,
                vm => vm.AddDialog,
                DoAddDialogAsync);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private static async Task DoAddDialogAsync(InteractionContext<CompetitionProxy, bool> interaction)
        {
            var competitionProxy = interaction.Input;

            var dialog = new ContentDialog
            {
                Content = new CompetitionProxyView(),
                DataContext = competitionProxy,
                Title = Languages.Resources.AddCompetition,
                PrimaryButtonText = Languages.Resources.Ok,
                DefaultButton = ContentDialogButton.Primary,
            };

            using var _ = competitionProxy.IsValid()
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(valid => dialog.IsPrimaryButtonEnabled = valid);

            var result = await dialog.ShowAsync().ConfigureAwait(false);

            interaction.SetOutput(result == ContentDialogResult.Primary);
        }

        private static async Task DoShowConfirmDeleteDialogAsync(InteractionContext<CompetitionProxy, bool> interaction)
        {
            var deleteDialogViewModel = new DeleteEntityDialogViewModel();

            var dialog = new ContentDialog
            {
                Title = Languages.Resources.DeleteCompetition,
                Content =
                    string.Format(CultureInfo.CurrentCulture, Languages.Resources.DeleteCompetitionContent,
                        interaction.Input.Name),
                PrimaryButtonText = Languages.Resources.Delete,
                PrimaryButtonCommand = deleteDialogViewModel.OkCommand,
                CloseButtonText = Languages.Resources.Delete,
                CloseButtonCommand = deleteDialogViewModel.CancelCommand
            };

            await dialog.ShowAsync().ConfigureAwait(false);

            interaction.SetOutput(deleteDialogViewModel.CanDelete);
        }

        private static async Task DoShowEditDialogAsync(InteractionContext<CompetitionProxy, Unit> interaction)
        {
            var competitionProxy = interaction.Input;

            var dialog = new ContentDialog
            {
                Content = new CompetitionProxyView(),
                DataContext = competitionProxy,
                Title = Languages.Resources.EditCompetition,
                PrimaryButtonText = Languages.Resources.Ok,
            };

            using var _ = competitionProxy.IsValid()
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(valid => dialog.IsPrimaryButtonEnabled = valid);

            await dialog.ShowAsync().ConfigureAwait(false);

            interaction.SetOutput(Unit.Default);
        }
    }
}
