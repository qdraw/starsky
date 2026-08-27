using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Starsky.Desktop.Windows;

[ExcludeFromCodeCoverage]
public partial class ErrorWindow : Window
{
    public ErrorWindow(string message)
    {
        InitializeComponent();
        ErrorText.Text = message;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
