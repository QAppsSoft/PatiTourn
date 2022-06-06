using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViewModels.Modules.Skaters;

namespace PatiTournApp.Modules.Skaters
{
    public partial class SkaterProxyView : ReactiveUserControl<SkaterProxy>
    {
        public SkaterProxyView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
