using System.Diagnostics;

namespace Starsky.Windows.Services;

public sealed class ExternalOpenService
{
    public Task OpenUriAsync(string uri)
    {
        Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        return Task.CompletedTask;
    }

    public Task OpenFileAsync(string filePath)
    {
        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        return Task.CompletedTask;
    }
}