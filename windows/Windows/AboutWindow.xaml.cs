using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Starsky.Desktop.Windows;

[ExcludeFromCodeCoverage]
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        VersionText.Text = $"Version {ApplicationInfo.Version}";
    }

    private void OK_Click(object sender, RoutedEventArgs e) => Close();
}
