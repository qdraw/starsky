namespace Starsky.Desktop.Services;

public enum MountWatcherStatus { Running, Stopped, NotInstalled, Unknown }

public interface IMountWatcherService
{
    Task<bool> EnableAsync();
    Task<bool> DisableAsync();
    MountWatcherStatus GetStatus();
    void StopSync();
}
