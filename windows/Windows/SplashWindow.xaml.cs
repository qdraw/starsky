using System.Windows;

namespace Starsky.Desktop.Windows;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
    }

    public void UpdateStatus(string message)
    {
        Dispatcher.Invoke(() => StatusText.Text = message);
    }
}
