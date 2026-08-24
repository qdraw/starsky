using System.Windows;
using Starsky.Desktop.Services;

namespace Starsky.Desktop.Windows;

public partial class UpdateWindow : Window
{
    private readonly UpdateService _updateService;

    public UpdateWindow(UpdateService updateService)
    {
        InitializeComponent();
        _updateService = updateService;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _updateService.RecordWarningShown();
        Close();
    }
}
