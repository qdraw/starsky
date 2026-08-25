using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;

namespace Starsky.Desktop.Windows;

[ExcludeFromCodeCoverage]
public partial class SettingsWindow : Window
{
    private readonly SettingsService _settings;
    private readonly RemoteUrlValidator _validator;
    private readonly WindowManager _windowManager;
    private bool _initializing;

    public SettingsWindow(SettingsService settings, RemoteUrlValidator validator, WindowManager windowManager)
    {
        InitializeComponent();
        _settings = settings;
        _validator = validator;
        _windowManager = windowManager;
        LoadSettings();
    }

    private void LoadSettings()
    {
        _initializing = true;

        var s = _settings.Current;
        LocalRadio.IsChecked = s.Mode == RuntimeMode.Local;
        RemoteRadio.IsChecked = s.Mode == RuntimeMode.Remote;
        UrlBox.Text = s.RemoteBaseUrl;
        UpdateCheckBox.IsChecked = s.UpdateCheckEnabled;

        SetRemoteControlsEnabled(s.Mode == RuntimeMode.Remote);

        _initializing = false;
    }

    private void SetRemoteControlsEnabled(bool enabled)
    {
        UrlBox.IsEnabled = enabled;
        SaveUrlButton.IsEnabled = enabled;
    }

    private void LocalRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (_initializing)
        {
	        return;
        }

        SetRemoteControlsEnabled(false);
        var wasRemote = _settings.Current.Mode == RuntimeMode.Remote;
        _settings.Current.Mode = RuntimeMode.Local;
        _settings.Save();
        if (wasRemote)
        {
	        _windowManager.ReopenAll();
        }
    }

    private void RemoteRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (_initializing)
        {
	        return;
        }

        SetRemoteControlsEnabled(true);
        _settings.Current.Mode = RuntimeMode.Remote;
        _settings.Save();
    }

    private async void SaveUrlButton_Click(object sender, RoutedEventArgs e)
    {
        SaveUrlButton.IsEnabled = false;
        FeedbackText.Text = "Validating…";

        var url = UrlBox.Text.Trim();
        var result = await _validator.ValidateAsync(url);

        if (result.Success)
        {
            _settings.Current.RemoteBaseUrl = url.TrimEnd('/');
            _settings.Save();
            FeedbackText.Text = "Setting is saved";
            FeedbackText.Foreground = System.Windows.Media.Brushes.Green;
            _windowManager.ReopenAll();
        }
        else
        {
            FeedbackText.Text = $"FAIL setting is not valid and NOT saved — {result.Error}";
            FeedbackText.Foreground = System.Windows.Media.Brushes.Red;
        }

        SaveUrlButton.IsEnabled = true;
    }

    private void UpdateCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_initializing)
        {
	        return;
        }

        _settings.Current.UpdateCheckEnabled = UpdateCheckBox.IsChecked == true;
        _settings.Save();
    }
}
