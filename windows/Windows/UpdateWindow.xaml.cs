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
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
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
