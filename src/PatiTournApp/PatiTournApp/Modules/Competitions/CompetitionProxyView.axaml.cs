using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViewModels.Modules.Competitions;

namespace PatiTournApp.Modules.Competitions
{
    public partial class CompetitionProxyView : ReactiveUserControl<CompetitionProxy>
    {
        public CompetitionProxyView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
