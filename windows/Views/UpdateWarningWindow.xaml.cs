using Microsoft.UI.Xaml;
using Starsky.Windows.Services;

namespace Starsky.Windows.Views;

public sealed partial class UpdateWarningWindow : Window
{
    private readonly AppController _controller;

    public UpdateWarningWindow(AppController controller)
    {
        _controller = controller;
        Title = "Current Version Is Outdated";
        InitializeComponent();
        Closed += OnClosed;
    }

    private async void ReleaseOverview_Click(object sender, RoutedEventArgs e)
    {
        await _controller.OpenReleasesAsync();
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void OnClosed(object sender, WindowEventArgs args)
    {
        await _controller.MarkUpdateWarningSeenAsync();
        _controller.ClearUpdateWarningWindow(this);
    }
}