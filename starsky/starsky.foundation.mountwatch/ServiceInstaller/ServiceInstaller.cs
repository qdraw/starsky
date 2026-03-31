using System;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.ServiceInstaller;

/// <summary>
///     Factory for OS-specific service installers
/// </summary>
public class ServiceInstaller(IWebLogger logger) : IServiceInstaller
{
	/// <summary>
	///     Install service for the current OS
	/// </summary>
	public async Task<bool> InstallAsync(string executablePath)
	{
		var installer = CreateInstaller();
		return await installer.InstallAsync(executablePath);
	}

	/// <summary>
	///     Uninstall service for the current OS
	/// </summary>
	public async Task<bool> UninstallAsync()
	{
		var installer = CreateInstaller();
		return await installer.UninstallAsync();
	}

	/// <summary>
	///     Start service for the current OS
	/// </summary>
	public async Task<bool> StartAsync()
	{
		var installer = CreateInstaller();
		return await installer.StartAsync();
	}

	/// <summary>
	///     Stop service for the current OS
	/// </summary>
	public async Task<bool> StopAsync()
	{
		var installer = CreateInstaller();
		return await installer.StopAsync();
	}

	/// <summary>
	///     Create the OS-specific installer
	/// </summary>
	private IOsServiceInstaller CreateInstaller()
	{
		if ( OperatingSystem.IsMacOS() )
		{
			return new MacOsServiceInstaller(logger);
		}

		if ( OperatingSystem.IsWindows() )
		{
			return new WindowsServiceInstaller(logger);
		}

		if ( OperatingSystem.IsLinux() )
		{
			return new LinuxServiceInstaller(logger);
		}

		throw new PlatformNotSupportedException("OS not supported for service installation");
	}
}
