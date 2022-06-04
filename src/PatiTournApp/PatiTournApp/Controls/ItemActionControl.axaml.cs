using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PatiTournApp.Controls
{
    public partial class ItemActionControl : UserControl
    {
        public ItemActionControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
