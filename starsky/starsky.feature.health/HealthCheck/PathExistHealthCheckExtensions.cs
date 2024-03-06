using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.foundation.platform.Interfaces;

namespace starsky.feature.health.HealthCheck
{
	public static class PathExistHealthCheckExtensions
	{
		public static IHealthChecksBuilder AddPathExistHealthCheck(
			this IHealthChecksBuilder builder,
			Action<PathExistOptions>? setup,
			IWebLogger logger, 
			string? name = null,
			HealthStatus? failureStatus = null,
			IEnumerable<string>? tags = null,
			TimeSpan? timeout = null)
		{
			var options = new PathExistOptions();
			setup?.Invoke(options);
			return builder.Add(new HealthCheckRegistration(name ?? "pathexist", sp =>
				new PathExistHealthCheck(options, logger), failureStatus, tags, timeout));
		}
	}
}
