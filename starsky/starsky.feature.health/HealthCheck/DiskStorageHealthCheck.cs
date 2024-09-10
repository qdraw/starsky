using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace starsky.feature.health.HealthCheck
{
	/// <summary>
	/// To Check if the disk exist on the system
	/// So when you enter F:\ on a windows system, it checks if the actual F:\ drive is mounted
	/// This also works on a *nix system
	/// </summary>
	public class DiskStorageHealthCheck : IHealthCheck
	{
		private readonly DiskStorageOptions _options;

		public DiskStorageHealthCheck(DiskStorageOptions options)
		{
			var diskStorageOptions = options;
			_options = diskStorageOptions ?? throw new ArgumentNullException(nameof(options));
		}

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

		private static (bool Exists, long ActualFreeMegabytes) GetSystemDriveInfo(string driveName)
		{
			DriveInfo[] drivesList;
			try
			{
				drivesList = DriveInfo.GetDrives();
			}
			catch ( Exception )
			{
				return (false, 0L);
			}

			var driveInfo = Array.Find(drivesList,
				drive => string.Equals(drive.Name, driveName,
					StringComparison.InvariantCultureIgnoreCase));
			return driveInfo?.AvailableFreeSpace != null
				? (true, driveInfo.AvailableFreeSpace / 1024L / 1024L)
				: (false, 0L);
		}
	}
}
