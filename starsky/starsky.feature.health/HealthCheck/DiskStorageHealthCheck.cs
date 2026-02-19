using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.foundation.platform.Interfaces;

namespace starsky.feature.health.HealthCheck;

/// <summary>
///     To Check if the disk exist on the system
///     So when you enter F:\ on a Windows system, it checks if the actual F:\ drive is mounted
///     This also works on a *nix system
/// </summary>
public class DiskStorageHealthCheck(DiskStorageOptions? options, IWebLogger logger) : IHealthCheck
{
	private readonly DiskStorageOptions _options =
		options ?? throw new ArgumentNullException(nameof(options));

	public Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default)
	{
		try
		{
			foreach ( var (driveName, num) in _options.ConfiguredDrives.Values )
			{
				var (exists4, actualFreeMegabytes4) = GetSystemDriveInfo(driveName);
				if ( !exists4 )
				{
					return Task.FromResult(new HealthCheckResult(
						context.Registration.FailureStatus,
						"Configured drive " + driveName + " is not present on system"));
				}

				if ( actualFreeMegabytes4 < num )
				{
					return Task.FromResult(new HealthCheckResult(
						context.Registration.FailureStatus,
						$"Minimum configured megabytes for disk {driveName} is {num} " +
						$"but actual free space are {actualFreeMegabytes4} megabytes"));
				}
			}

			return Task.FromResult(HealthCheckResult.Healthy());
		}
		catch ( Exception ex )
		{
			return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus,
				null, ex));
		}
	}

	/// <summary>
	///     NOTE:
	///     On non-Windows platforms, DriveInfo.GetDrives() may throw when encountering
	///     mounted volumes with Unix-style paths (e.g. under /Volumes on macOS).
	///     This is due to platform-specific drive name normalization in .NET.
	/// </summary>
	/// <param name="driveName">Path of drive</param>
	/// <returns>Exists and ActualFreeMegabytes</returns>
	private (bool Exists, long ActualFreeMegabytes) GetSystemDriveInfo(string driveName)
	{
		return OperatingSystem.IsWindows()
			? GetWindowsDriveInfo(driveName)
			: GetUnixDriveInfo(driveName);
	}

	internal (bool exists, long actualFreeMegabytes) GetWindowsDriveInfo(string driveName)
	{
		try
		{
			var driveInfo = DriveInfo.GetDrives()
				.FirstOrDefault(d =>
					string.Equals(d.Name, driveName, StringComparison.OrdinalIgnoreCase));

			return driveInfo != null
				? ( true, driveInfo.AvailableFreeSpace / 1024 / 1024 )
				: ( false, 0L );
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, "[DiskStorageHealthCheck] Error retrieving Windows drive info");
			return ( false, 0L );
		}
	}

	internal (bool Exists, long ActualFreeMegabytes) GetUnixDriveInfo(string path)
	{
		try
		{
			if ( !Directory.Exists(path) )
			{
				return ( false, 0L );
			}

			var drive = new DriveInfo(path);
			return ( true, drive.AvailableFreeSpace / 1024 / 1024 );
		}
		catch ( Exception exception )
		{
			logger.LogError(exception, "[DiskStorageHealthCheck] Error retrieving Unix disk info");
			return ( false, 0L );
		}
	}
}
