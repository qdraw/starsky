using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.mountwatch.ServiceInstaller.Helpers;
using starsky.foundation.mountwatch.ServiceInstaller.Interfaces;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.mountwatch.ServiceInstaller;

/// <summary>
///     Factory for OS-specific service installers
/// </summary>
[Service(typeof(IServiceInstaller), InjectionLifetime = InjectionLifetime.Scoped)]
public class ServiceInstaller(ISelectorStorage selectorStorage, IWebLogger logger)
	: IServiceInstaller
{
	private readonly Func<OSPlatform> _platformResolver = OperatingSystemHelper.GetPlatform;

	internal ServiceInstaller(ISelectorStorage selectorStorage, IWebLogger logger,
		Func<OSPlatform> platformResolver) :
		this(selectorStorage, logger)
	{
		_platformResolver = platformResolver;
	}

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

	public bool? PreflightChecks()
	{
		return new MacOsFullDiskAccess(selectorStorage, logger).CheckMacOsFullDiskAccessOnStartup();
	}

	/// <summary>
	///     Create the OS-specific installer
	/// </summary>
	private IOsServiceInstaller CreateInstaller()
	{
		var platform = _platformResolver();

		if ( platform == OSPlatform.OSX )
		{
			return new MacOsServiceInstaller(logger);
		}

		if ( platform == OSPlatform.Windows )
		{
			return new WindowsServiceInstaller(logger);
		}

		if ( platform == OSPlatform.Linux )
		{
			return new LinuxServiceInstaller(logger);
		}

		throw new PlatformNotSupportedException("OS not supported for service installation");
	}
}
