using Microsoft.UI.Xaml;

namespace Starsky.Windows.Views;

public sealed partial class ErrorWindow : Window
{
    public ErrorWindow(string error)
    {
        Title = "Error";
        InitializeComponent();
        ErrorTextBox.Text = error;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}