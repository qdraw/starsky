using System.Threading.Tasks;

namespace starsky.foundation.mountwatch.ServiceInstaller.Interfaces;

/// <summary>
///     Abstraction for installing/uninstalling the mount watcher as an OS service
/// </summary>
public interface IServiceInstaller
{
	/// <summary>
	///     Install the mount watcher as an OS service (launchd/systemd/Windows Service)
	/// </summary>
	/// <param name="executablePath">Full path to the CLI executable</param>
	/// <returns>True on success</returns>
	Task<bool> InstallAsync(string executablePath);

	/// <summary>
	///     Uninstall the OS service
	/// </summary>
	/// <returns>True on success</returns>
	Task<bool> UninstallAsync();

	/// <summary>
	///     Start the OS service
	/// </summary>
	/// <returns>True on success</returns>
	Task<bool> StartAsync();

	/// <summary>
	///     Stop the OS service
	/// </summary>
	/// <returns>True on success</returns>
	Task<bool> StopAsync();
}
