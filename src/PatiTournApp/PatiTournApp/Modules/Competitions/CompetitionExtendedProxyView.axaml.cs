using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PatiTournApp.Modules.Competitions
{
    public partial class CompetitionExtendedProxyView : UserControl
    {
        public CompetitionExtendedProxyView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
