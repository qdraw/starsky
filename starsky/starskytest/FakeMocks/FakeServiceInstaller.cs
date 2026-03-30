using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.Interfaces;

namespace starskytest.FakeMocks;

public class FakeServiceInstaller : IServiceInstaller
{
	public List<string> InstalledPaths { get; } = new();
	public int UninstallCount { get; private set; }
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
}
