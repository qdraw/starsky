using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Starsky.Windows.Models;
using Starsky.Windows.Services;
using Windows.System;

namespace Starsky.Windows.Views;

public sealed partial class SettingsWindow : Window
{
    private readonly AppController _controller;
    private bool _isLoading;

    public SettingsWindow(AppController controller)
    {
        _controller = controller;
        Title = "Settings";
        InitializeComponent();
        Closed += OnClosed;
    }

    public async Task InitializeAsync()
    {
        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        _isLoading = true;

        LocalRadioButton.IsEnabled = false;
        RemoteRadioButton.IsEnabled = false;
        UpdatePolicyToggle.IsEnabled = false;

        LocalRadioButton.IsChecked = _controller.Settings.Mode == LocationMode.Local;
        RemoteRadioButton.IsChecked = _controller.Settings.Mode == LocationMode.Remote;
        RemoteUrlTextBox.Text = _controller.Settings.RemoteUrl ?? string.Empty;
        RemoteUrlTextBox.IsEnabled = _controller.IsRemoteMode;
        UpdatePolicyToggle.IsOn = _controller.Settings.UpdatePolicyEnabled;
        RemoteStatusTextBlock.Text = "Do enter the main domain starsky is running on";

        LocalRadioButton.IsEnabled = true;
        RemoteRadioButton.IsEnabled = true;
        UpdatePolicyToggle.IsEnabled = true;

        _isLoading = false;
        await _controller.PersistSettingsWindowStateAsync(this);
    }

    private async void RootPanel_OnLoaded(object sender, RoutedEventArgs e)
    {
        await RefreshAsync();
    }

    private async void LocalRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        await _controller.SwitchModeAsync(LocationMode.Local);
        await RefreshAsync();
    }

    private async void RemoteRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        await _controller.SwitchModeAsync(LocationMode.Remote);
        await RefreshAsync();
    }

    private async void RemoteUrlTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        await SaveRemoteUrlAsync();
    }

    private async void RemoteUrlTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            await SaveRemoteUrlAsync();
        }
    }

    private async Task SaveRemoteUrlAsync()
    {
        if (_isLoading || !_controller.IsRemoteMode)
        {
            return;
        }

        var result = await _controller.ValidateAndSaveRemoteUrlAsync(RemoteUrlTextBox.Text);
        RemoteStatusTextBlock.Text = result.IsValid
            ? "Setting is saved"
            : "FAIL setting is not valid and NOT saved";

        if (result.IsValid)
        {
            await _controller.SwitchModeAsync(LocationMode.Remote);
        }
    }

    private async void UpdatePolicyToggle_OnToggled(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        await _controller.SetUpdatePolicyAsync(UpdatePolicyToggle.IsOn);
    }

    private async void OnClosed(object sender, WindowEventArgs args)
    {
        await _controller.PersistSettingsWindowStateAsync(this);
        _controller.RemoveSettingsWindow(this);
    }
}