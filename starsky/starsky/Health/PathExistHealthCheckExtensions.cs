using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace starsky.Health
{
	public static class PathExistHealthCheckExtensions
	{
		public static IHealthChecksBuilder AddPathExistHealthCheck(
			this IHealthChecksBuilder builder,
			Action<PathExistOptions> setup,
			string name = null,
			HealthStatus? failureStatus = null,
			IEnumerable<string> tags = null,
			TimeSpan? timeout = null)
		{
			var options = new PathExistOptions();
			setup?.Invoke(options);
			return builder.Add(new HealthCheckRegistration(name ?? "pathexist", sp => 
				(IHealthCheck) new PathExistHealthCheck(options), failureStatus, tags, timeout));
		}
	}
}
