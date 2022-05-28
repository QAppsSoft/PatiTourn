using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PatiTournApp.Modules.Competitions
{
    public partial class CompetitionProxyEditorView : UserControl
    {
        public CompetitionProxyEditorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
