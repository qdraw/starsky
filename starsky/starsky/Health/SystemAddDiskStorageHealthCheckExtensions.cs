using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace starsky.Health
{
	public static class SystemAddDiskStorageHealthCheckExtensions
	{
		public static IHealthChecksBuilder AddDiskStorageHealthCheck(
			this IHealthChecksBuilder builder,
			Action<DiskStorageOptions> setup,
			string name = null,
			HealthStatus? failureStatus = null,
			IEnumerable<string> tags = null,
			TimeSpan? timeout = null)
		{
			var options = new DiskStorageOptions();
			setup?.Invoke(options);
			return builder.Add(new HealthCheckRegistration(name ?? "diskstorage", sp => 
				(IHealthCheck) new DiskStorageHealthCheck(options), failureStatus, tags, timeout));
		}
	}
}
