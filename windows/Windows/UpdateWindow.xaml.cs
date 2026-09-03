using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Starsky.Desktop.Services;

namespace Starsky.Desktop.Windows;

[ExcludeFromCodeCoverage]
public partial class UpdateWindow : Window
{
    private readonly UpdateService _updateService;

    public UpdateWindow(UpdateService updateService)
    {
        InitializeComponent();
        _updateService = updateService;

        if (updateService.IsGitHubFallbackUpdate)
        {
            UpdateButton.Content = "Go to Release";
            DescriptionText.Text = "Click 'Go to Release' to open the GitHub release page and download the installer manually.";
        }
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_updateService.IsGitHubFallbackUpdate)
        {
            _updateService.RecordWarningShown();
            Process.Start(new ProcessStartInfo(_updateService.PendingGitHubReleaseUrl!) { UseShellExecute = true });
            Close();
            return;
        }

        try
        {
            IsEnabled = false;
            await _updateService.ApplyUpdateAsync();
        }
        catch (Exception ex)
        {
            IsEnabled = true;
            MessageBox.Show($"Update failed: {ex.Message}", "Update Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _updateService.RecordWarningShown();
        Close();
    }
}
