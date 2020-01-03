using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TRADDataMonitor
{
    public class DataWindow : Window
    {
        public DataWindow()
        {
            this.InitializeComponent();
            this.DataContext = new DataWindowViewModel();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
