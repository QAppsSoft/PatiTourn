using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Mixins;
using Avalonia.Threading;
using Common.Extensions;
using ReactiveUI;
using ViewModels;
using FluentAvalonia.UI.Controls;
using PatiTournApp.Modules.Competitions;
using ReactiveUI.Validation.Extensions;
using ViewModels.Modules.Competitions;

namespace PatiTournApp.Views
{
    public partial class MainWindow : CoreWindow, IViewFor<MainWindowViewModel>
    {
        public static readonly StyledProperty<MainWindowViewModel?> ViewModelProperty = AvaloniaProperty
            .Register<CoreWindow, MainWindowViewModel?>(nameof(ViewModel));

        public MainWindow()
        {
            InitializeComponent();
            
            this.GetObservable(DataContextProperty).Subscribe(OnDataContextChanged);
            this.GetObservable(ViewModelProperty).Subscribe(OnViewModelChanged);

            this.BindInteraction(ViewModel,
                vm => vm.CompetitionsDialog,
                DoShowCompetitionsDialogAsync);
            
            this.WhenAnyValue(x => x.ViewModel)
                .ToUnit()
                .InvokeCommand(this, x => x.ViewModel.CompetitionsViewModel.Refresh);
        }

        private static async Task DoShowCompetitionsDialogAsync(InteractionContext<CompetitionsViewModel, CompetitionProxy> interaction)
        {
            var competitionsViewModel = interaction.Input;

            var dialog = new ContentDialog
            {
                Content = new CompetitionsView(),
                DataContext = competitionsViewModel,
                Title = "Search competition",
                PrimaryButtonText = "Ok"
            };

            using var _ = competitionsViewModel
                .WhenAnyValue(competitionsViewModel => competitionsViewModel.SelectedProxy)
                .Select(proxy => proxy != null)
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(valid => dialog.IsPrimaryButtonEnabled = valid);

            await dialog.ShowAsync().ConfigureAwait(false);

            interaction.SetOutput(competitionsViewModel.SelectedProxy!); // Ignored because the dialog cant be closed if a CompetitionProxy is not selected
        }

        #region IViewFor<MainWindowViewModel> implementation

        /// <summary>
        /// The ViewModel.
        /// </summary>
        public MainWindowViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainWindowViewModel?)value;
        }

        private void OnDataContextChanged(object? value)
        {
            if (value is MainWindowViewModel viewModel)
            {
                ViewModel = viewModel;
            }
            else
            {
                ViewModel = null;
            }
        }

        private void OnViewModelChanged(object? value)
        {
            if (value == null)
            {
                ClearValue(DataContextProperty);
            }
            else if (DataContext != value)
            {
                DataContext = value;
            }
        }

        #endregion IViewFor<MainWindowViewModel> implementation

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            SetTitleBar(this);
        }

        private void SetTitleBar(CoreWindow cw)
        {
            // Grab the ICoreApplicationViewTitleBar attached to the CoreWindow object
            // On Windows, this will never be null. On Mac/Linux, it will be - make sure
            // to null check
            var titleBar = cw.TitleBar;
            if (titleBar != null)
            {
                // Tell CoreWindow we want to remove the default TitleBar and set our own
                titleBar.ExtendViewIntoTitleBar = true;

                // Call SetTitleBar to tell CoreWindow the element we want to use as the TitleBar
                cw.SetTitleBar(TitleBarHost);

                // Set the margin of the Custom TitleBar so it doesn't overlap with the CaptionButtons
                TitleBarHost.Margin = new Thickness(0, 0, titleBar.SystemOverlayRightInset, 0);

                // You can optionally subscribe to LayoutMetricsChanged to be notified of when TitleBar bounds change
                // Right now, it doesn't do much. It will be more important when RTL layouts are supported as that will
                // notify you of a change in the SystemOverlay[Left/Right]Inset properties and require adjusting
                // that margin
            }
        }
    }
}
