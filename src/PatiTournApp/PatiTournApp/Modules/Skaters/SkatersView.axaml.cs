using System;
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
using ViewModels.Modules.Skaters;

namespace PatiTournApp.Modules.Skaters
{
    public partial class SkatersView : ReactiveUserControl<SkatersViewModel>
    {
        public SkatersView()
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

        private static async Task DoAddDialogAsync(InteractionContext<SkaterProxy, bool> interaction)
        {
            var skaterProxy = interaction.Input;

            var dialog = new ContentDialog
            {
                Content = new SkaterProxyView(),
                DataContext = skaterProxy,
                Title = "Add skater",
                PrimaryButtonText = "Ok",
                DefaultButton = ContentDialogButton.Primary,
            };

            using var _ = skaterProxy.IsValid()
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(valid => dialog.IsPrimaryButtonEnabled = valid);

            var result = await dialog.ShowAsync().ConfigureAwait(false);

            interaction.SetOutput(result == ContentDialogResult.Primary);
        }

        private static async Task DoShowConfirmDeleteDialogAsync(InteractionContext<SkaterProxy, bool> interaction)
        {
            var deleteDialogViewModel = new DeleteEntityDialogViewModel();

            var dialog = new ContentDialog
            {
                Title = "Delete skater",
                Content = $"Do you want to delete the \"{interaction.Input.Name}\" skater.",
                PrimaryButtonText = "Delete",
                PrimaryButtonCommand = deleteDialogViewModel.OkCommand,
                CloseButtonText = "Cancel",
                CloseButtonCommand = deleteDialogViewModel.CancelCommand
            };

            await dialog.ShowAsync().ConfigureAwait(false);

            interaction.SetOutput(deleteDialogViewModel.CanDelete);
        }

        private static async Task DoShowEditDialogAsync(InteractionContext<SkaterProxy, Unit> interaction)
        {
            var skaterProxy = interaction.Input;

            var dialog = new ContentDialog
            {
                Content = new SkaterProxyView(),
                DataContext = skaterProxy,
                Title = "Edit skater",
                PrimaryButtonText = "Ok",
                DefaultButton = ContentDialogButton.Primary,
            };

            using var _ = skaterProxy.IsValid()
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(valid => dialog.IsPrimaryButtonEnabled = valid);

            await dialog.ShowAsync().ConfigureAwait(false);

            interaction.SetOutput(Unit.Default);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
