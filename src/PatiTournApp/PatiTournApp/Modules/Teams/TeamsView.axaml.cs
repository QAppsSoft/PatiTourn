using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Common.Extensions;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ViewModels.Dialogs;
using ViewModels.Modules.Teams;

namespace PatiTournApp.Modules.Teams
{
    public partial class TeamsView : ReactiveUserControl<TeamsViewModel>
    {
        public TeamsView()
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

            this.WhenAnyValue(x => x.ViewModel)
                .ToUnit()
                .InvokeCommand(this, x => x.ViewModel.Refresh);
        }

        private static async Task DoAddDialogAsync(InteractionContext<TeamProxy, bool> interaction)
        {
            var teamProxy = interaction.Input;

            var dialog = new ContentDialog
            {
                Content = new TeamProxyView(),
                DataContext = teamProxy,
                Title = Languages.Resources.AddTeam,
                PrimaryButtonText = Languages.Resources.Ok,
                DefaultButton = ContentDialogButton.Primary,
            };

            using var _ = teamProxy.IsValid()
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(valid => dialog.IsPrimaryButtonEnabled = valid);

            var result = await dialog.ShowAsync().ConfigureAwait(false);

            interaction.SetOutput(result == ContentDialogResult.Primary);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private static async Task DoShowConfirmDeleteDialogAsync(InteractionContext<TeamProxy, bool> interaction)
        {
            var deleteDialogViewModel = new DeleteEntityDialogViewModel();

            var dialog = new ContentDialog
            {
                Title = Languages.Resources.DeleteTeam,
                Content =
                    string.Format(CultureInfo.CurrentCulture, Languages.Resources.DeleteTeamContent,
                        interaction.Input.Name),
                PrimaryButtonText = Languages.Resources.Delete,
                PrimaryButtonCommand = deleteDialogViewModel.OkCommand,
                CloseButtonText = Languages.Resources.Cancel,
                CloseButtonCommand = deleteDialogViewModel.CancelCommand,
            };

            await dialog.ShowAsync().ConfigureAwait(false);

            interaction.SetOutput(deleteDialogViewModel.CanDelete);
        }

        private static async Task DoShowEditDialogAsync(InteractionContext<TeamProxy, Unit> interaction)
        {
            var teamProxy = interaction.Input;

            var dialog = new ContentDialog
            {
                Content = new TeamProxyView(),
                DataContext = teamProxy,
                Title = Languages.Resources.EditTeam,
                PrimaryButtonText = Languages.Resources.Ok,
                DefaultButton = ContentDialogButton.Primary,
            };

            using var _ = teamProxy.IsValid()
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(valid => dialog.IsPrimaryButtonEnabled = valid);

            await dialog.ShowAsync().ConfigureAwait(false);

            interaction.SetOutput(Unit.Default);
        }
    }
}
