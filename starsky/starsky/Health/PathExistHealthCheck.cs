using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

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
			var resultsList = _options.ConfiguredPaths.Select(path => new StorageHostFullPathFilesystem().IsFolderOrFile(path)).ToList();

			if ( !resultsList.Any() )
				return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus,
					$"Not configured"));

			return Task.FromResult(resultsList.Any(p => p == FolderOrFileModel.FolderOrFileTypeList.Deleted) ? 
				new HealthCheckResult(context.Registration.FailureStatus, $"Configured path is not present on system") : 
				HealthCheckResult.Healthy());
		}

	}
}
