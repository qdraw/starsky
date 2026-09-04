using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace Starsky.Desktop.Windows;

[ExcludeFromCodeCoverage]
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

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        Hide();
    }
}
