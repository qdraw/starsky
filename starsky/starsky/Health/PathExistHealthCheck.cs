using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starskycore.Models;
using starskycore.Services;

namespace starsky.Health
{
	public class PathExistHealthCheck : IHealthCheck
	{
		private readonly PathExistOptions _options;

		public PathExistHealthCheck(PathExistOptions options)
		{
			var diskStorageOptions = options;
			_options = diskStorageOptions ?? throw new ArgumentNullException(nameof(options));
		}

		public Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context,
			CancellationToken cancellationToken = default)
		{
			foreach ( var path in _options.ConfiguredPaths )
			{
				var isFolderOrFile = new StorageHostFullPathFilesystem().IsFolderOrFile(path);
				return Task.FromResult(isFolderOrFile == FolderOrFileModel.FolderOrFileTypeList.Deleted ? 
					new HealthCheckResult(context.Registration.FailureStatus, $"Configured {path} is not present on system") : 
					HealthCheckResult.Healthy());
			}
			return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus));
		}

	}
}
