using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;

namespace starskytest.FakeMocks;

public class FakeServiceInstaller : IServiceInstaller
{
	public List<string> InstalledPaths { get; } = new();
	public int UninstallCount { get; private set; }
	public int StartCount { get; private set; }
	public int StopCount { get; private set; }
	public bool ReturnValue { get; set; } = true;

	public Task<bool> InstallAsync(string executablePath)
	{
		InstalledPaths.Add(executablePath);
		return Task.FromResult(ReturnValue);
	}

	public Task<bool> UninstallAsync()
	{
		UninstallCount++;
		return Task.FromResult(ReturnValue);
	}

	public Task<bool> StartAsync()
	{
		StartCount++;
		return Task.FromResult(ReturnValue);
	}

	public Task<bool> StopAsync()
	{
		StopCount++;
		return Task.FromResult(ReturnValue);
	}

	public bool? PreflightChecks()
	{
		return true;
	}

	public Task<(bool installed, bool running)> StatusAsync()
	{
		var installed = InstalledPaths.Count > 0;
		var running = StartCount > 0;
		return Task.FromResult((installed, running));
	}
}
