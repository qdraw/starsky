using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

namespace starsky.feature.health.HealthCheck
{
	/// <summary>
	/// Checks if the path exist
	/// </summary>
	public class PathExistHealthCheck : IHealthCheck
	{
		private readonly PathExistOptions _options;
		private readonly IWebLogger _logger;

		public PathExistHealthCheck(PathExistOptions options, IWebLogger logger)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger;
		}

		public Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context,
			CancellationToken cancellationToken = default)
		{
			var resultsList = _options.ConfiguredPaths.Select(path =>
				new StorageHostFullPathFilesystem(_logger)
					.IsFolderOrFile(path)).ToList();

			if ( resultsList.Count == 0 )
				return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus,
					$"Not configured"));

			return Task.FromResult(
				resultsList.Exists(p => p == FolderOrFileModel.FolderOrFileTypeList.Deleted)
					? new HealthCheckResult(context.Registration.FailureStatus,
						$"Configured path is not present on system")
					: HealthCheckResult.Healthy("Configured path is present"));
		}
	}
}
