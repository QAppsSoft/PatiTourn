using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Mixins;
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

            this.WhenActivated(disposable =>
            {
                ViewModel!
                    .ConfirmDeleteDialog
                    .RegisterHandler(DoShowConfirmDeleteDialogAsync)
                    .DisposeWith(disposable);

                ViewModel!
                    .EditDialog
                    .RegisterHandler(DoShowEditDialogAsync)
                    .DisposeWith(disposable);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private static async Task DoShowConfirmDeleteDialogAsync(InteractionContext<CompetitionProxy, bool> interaction)
        {
            var deleteDialogViewModel = new DeleteEntityDialogViewModel();

            var dialog = new ContentDialog
            {
                Title = "Delete competition",
                Content = $"Do you want to delete the \"{interaction.Input.Name}\" competition.",
                PrimaryButtonText = "Delete",
                PrimaryButtonCommand = deleteDialogViewModel.OkCommand,
                CloseButtonText = "Cancel",
                CloseButtonCommand = deleteDialogViewModel.CancelCommand
            };

            await dialog.ShowAsync();

            interaction.SetOutput(deleteDialogViewModel.CanDelete);
        }

        private static async Task DoShowEditDialogAsync(InteractionContext<CompetitionProxy, Unit> interaction)
        {
            var competitionProxy = interaction.Input;

            var dialog = new ContentDialog
            {
                Content = new CompetitionProxyView(),
                DataContext = competitionProxy,
                Title = "Edit competition",
                PrimaryButtonText = "Ok"
            };

            using var _ = competitionProxy.IsValid()
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(valid => dialog.IsPrimaryButtonEnabled = valid);

            await dialog.ShowAsync();

            interaction.SetOutput(Unit.Default);
        }
    }
}
