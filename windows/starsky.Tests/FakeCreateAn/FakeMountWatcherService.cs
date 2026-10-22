using Starsky.Desktop.Services;

namespace starsky.Tests.FakeCreateAn;

internal sealed class FakeMountWatcherService : IMountWatcherService
{
    public bool EnableCalled;
    public bool DisableCalled;
    public bool StopSyncCalled;
    public bool EnableResult = true;
    public bool DisableResult = true;
    public MountWatcherStatus StatusResult = MountWatcherStatus.NotInstalled;

    public Task<bool> EnableAsync() { EnableCalled = true; return Task.FromResult(EnableResult); }
    public Task<bool> DisableAsync() { DisableCalled = true; return Task.FromResult(DisableResult); }
    public MountWatcherStatus GetStatus() => StatusResult;
    public void StopSync() { StopSyncCalled = true; }
}
