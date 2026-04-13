using System.Threading.Tasks;

namespace starsky.foundation.mountwatch.ServiceInstaller.Interfaces;

/// <summary>
///     Abstraction for OS-specific service installation
/// </summary>
internal interface IOsServiceInstaller
{
	/// <summary>
	///     Install the service for this OS
	/// </summary>
	Task<bool> InstallAsync(string executablePath);

	/// <summary>
	///     Uninstall the service for this OS
	/// </summary>
	Task<bool> UninstallAsync();

	/// <summary>
	///     Start the service for this OS
	/// </summary>
	Task<bool> StartAsync();

	/// <summary>
	///     Stop the service for this OS
	/// </summary>
	Task<bool> StopAsync();

	/// <summary>
	/// Returns installation and running status: (installed, running)
	/// </summary>
	Task<(bool installed, bool running)> StatusAsync();
}
