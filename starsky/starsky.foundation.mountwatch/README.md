# starsky.foundation.mountwatch

Introduction
------------
`starsky.foundation.mountwatch` detects external storage mounts (USB drives, cameras, card readers,
network mounts depending on platform) and exposes mount detection events to the Starsky system (
importer, background services, and tooling). It supports macOS Disk Arbitration integration and
provides platform-specific service installers for macOS (launch agents / plist), Linux (systemd
unit), and Windows (service wrappers).

Goals

- Detect new external mounts and notify downstream consumers reliably.
- Provide a resilient fallback when platform APIs are not available (polling loop).
- Be testable via dependency injection and fakes to avoid touching host system APIs in unit tests.
- Offer installers/uninstallers for each OS to register the watcher as a background service/daemon.

Table of contents

- Architecture & components
- Events & public API
- Threading and polling
- Platform specifics (macOS / Linux / Windows)
- Service installation
- CLI usage
- Configuration & conventions
- Testing & CI guidance
- Troubleshooting
- Building & running
- Contributing

Architecture & components
-------------------------
The library is centered around a platform-appropriate `BaseMountWatcher` implementation and several
supporting components:

- `BaseMountWatcher` (abstract): shared logic - known volumes tracking, normalization helpers,
  lifecycle (Start/Stop), and `MountDetected` event.
- `MacMountWatcher`: macOS implementation that integrates Disk Arbitration via
  `IMacMountWatcherSystem` and provides a polling safety net (`RunBackupPollingLoop`).
- `WindowsMountWatcher`: Windows implementation that uses WMI (`ManagementEventWatcher`) with a
  polling fallback.
- `LinuxMountWatcher`: Linux implementation that uses udev (libudev) with a polling fallback.
- `MountWatcherCli`: CLI orchestrator; subscribes to `MountDetected` and delegates import work to
  `IImport`/`ImportCli` using background tasks.
- `MountDetector`: Detects camera storage (DCIM folders) on a mount.
- Service installers: `MacOsServiceInstaller`, `LinuxServiceInstaller`, `WindowsServiceInstaller` —
  generate and manage OS-specific service artifacts.
- Helpers: `ServiceInstallerHelper`, `WatchServiceName` (canonical service names such as
  `GetSystemDName()`).

Events & public API
-------------------

- Event: `event EventHandler<MountDetectedEventArgs> MountDetected` —
  `MountDetectedEventArgs.MountPath` contains the normalized mount path.
- Lifecycle methods: `StartAsync()`, `Stop()` / `StopAsync()`.
- Scanning: `EmitNewExternalMounts(string reason)`, `GetMountedVolumes()` (platform-specific).
- Service installer API: `InstallAsync(string executablePath)`, `UninstallAsync()`, `StartAsync()`,
  `StopAsync()`.

Threading and polling behavior
-----------------------------

- Event callbacks and the polling loop may run on background threads.
- `RunBackupPollingLoop()` (macOS example): loops while `IsRunning`, calls
  `EmitNewExternalMounts("polling backup")` inside try/catch and sleeps `PollIntervalMs` between
  iterations. This loop must never crash the watcher.
- Consumers should expect `MountDetected` handlers to be invoked concurrently and should offload
  long work using `Task.Run` or other background mechanisms.

Platform specifics
------------------

- macOS
    - Uses Disk Arbitration when available via P/Invoke. `IMacMountWatcherSystem` abstracts system
      calls for testing.
    - `OnDiskAppeared` and `OnDiskDisappeared` update known mounts and call
      `EmitNewExternalMounts()`.
    - `Stop()` must unschedule the DASession and stop the CFRunLoop (
      `DASessionUnscheduleWithRunLoop`, `CFRunLoopStop`).

- Linux
    - Uses systemd unit templates for service installation. The canonical name for systemd units is
      provided by `WatchServiceName.GetSystemDName()`.
    - Installer writes to `/etc/systemd/system/` and the user's `~/.config/systemd/user/` when
      appropriate.

- Windows
    - Installer wraps service manager calls (sc.exe) or platform wrappers. Error branches are logged
      via `IWebLogger.LogError` for both non-exception returns and thrown exceptions.

Service installation
--------------------
Each platform includes an installer that generates the appropriate unit/plist/service:

- macOS: LaunchAgent plist in `~/Library/LaunchAgents/` (or system equivalents) with notes about
  Full Disk Access and `launchctl load` to enable.
- Linux: systemd unit file at `/etc/systemd/system/{GetSystemDName()}.service` with recommended
  `systemctl daemon-reload` and enable/start instructions.
- Windows: `sc.exe` service creation and standard start/stop instructions.

CLI usage
---------
Typical CLI usage (from `starskymountwatchercli`):

```bash
# Run the watcher
starskymountwatchercli

# Install as OS service
starskymountwatchercli --install

# Uninstall
starskymountwatchercli --uninstall

# Help
starskymountwatchercli --help
```

Configuration & conventions
-------------------------

- Use `WatchServiceName.GetSystemDName()` when forming or asserting systemd unit filenames in code
  and tests.
- `PollIntervalMs` controls polling frequency for the backup loop; tests may adjust this value.
- Log via `IWebLogger` and in tests use `FakeIWebLogger` to capture `TrackedInformation` and
  `TrackedExceptions`.

Testing & CI guidance
---------------------

- Inject side-effecting dependencies (`IStorage`, `IWebLogger`, `IMacMountWatcherSystem`, importers)
  so unit tests don't touch system resources.
- Replace brittle sleep-based waits with deterministic synchronization:
    - Use `TaskCompletionSource` to wait for a specific number of `MountDetected` events.
    - Use thread-safe collections (`ConcurrentQueue`) to collect events.
    - Example pattern: subscribe to `MountDetected`, push to queue, call `tcs.TrySetResult(true)`
      when expected events arrive, await `Task.WhenAny(tcs.Task, Task.Delay(timeout))`.
- To avoid CI flakiness, prefer adding an internal synchronous test hook such as
  `RunBackupIterationForTests()` that runs one iteration of the polling logic; tests call it
  directly to eliminate background scheduling races.
- Ensure `FakeIStorage.ExistFile` normalizes paths (remove trailing slashes) to match production
  `UninstallAsync` checks.

Troubleshooting
---------------

- Missing systemd unit removals in tests: ensure the fake storage uses the exact path created by
  `WatchServiceName.GetSystemDName()`.
- Mount events not observed in tests: use `TaskCompletionSource` + `ConcurrentQueue` rather than
  fixed delays.
- macOS cleanup not invoked: tests may need to set non-zero `IntPtr` placeholders for `_session`/_
  runLoop/_runLoopMode` when using reflection to emulate a DASession; better is to inject `
  IMacMountWatcherSystem` fakes that track `DASessionUnscheduleWithRunLoop` and `CFRunLoopStop`
  calls.

How to run tests
----------------
Run tests from the repository root:

```bash
dotnet test starsky.sln
```

To diagnose flaky tests:

- Re-run with `dotnet test --filter FullyQualifiedName~YourTestName`.
- Increase timeouts or use a test-only synchronous hook as described above.

Project layout
--------------
Key files and folders:

```
starsky.foundation.mountwatch/
├── Interfaces/
│   ├── IMountWatcher.cs
│   ├── IMountDetector.cs
│   └── IServiceInstaller.cs
├── Services/
│   ├── MountDetector.cs
│   ├── MacMountWatcher.cs
│   ├── WindowsMountWatcher.cs
│   ├── LinuxMountWatcher.cs
│   ├── MountWatcherFactory.cs
│   ├── MountWatcherCli.cs
│   └── ServiceInstaller.cs
└── starsky.foundation.mountwatch.csproj

starskymountwatchercli/
├── Program.cs
└── starskymountwatchercli.csproj

starskytest/
├── starsky.foundation.mountwatch/
│   └── Services/
└── FakeMocks/
```

Building and running
--------------------
Build the projects:

```bash
dotnet build starsky.foundation.mountwatch/starsky.foundation.mountwatch.csproj
dotnet build starskymountwatchercli/starskymountwatchercli.csproj
```

Run tests:

```bash
dotnet test starskytest/starskytest.csproj --filter "FullyQualifiedName~MountWatch"
```

Run the watcher (foreground):

```bash
dotnet run --project starskymountwatchercli
```

Contributing and recommended improvements
----------------------------------------

- Add an internal test-only API on `MacMountWatcher`: `internal void RunBackupIterationForTests()`
  to run a single iteration deterministically.
- Use `InternalsVisibleTo` for the test assembly to avoid reflection in tests.
- Consider an `IPollingTimer`/`ISleeper` abstraction so tests can control or stub sleeps.
- Improve logging by including the `reason` parameter when emitting mounts to make CI debugging
  easier.

License & support
-----------------
See the top-level repository `README.md` for licensing and CI build details.


