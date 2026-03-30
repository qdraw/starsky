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
}
