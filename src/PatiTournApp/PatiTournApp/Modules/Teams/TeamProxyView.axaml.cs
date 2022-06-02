using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PatiTournApp.Modules.Teams
{
    public partial class TeamProxyView : UserControl
    {
        public TeamProxyView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
