using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.foundation.platform.Interfaces;

namespace starsky.feature.health.HealthCheck;

public static class DiskStorageHealthCheckExtensions
{
	public static IHealthChecksBuilder AddDiskStorageHealthCheck(
		this IHealthChecksBuilder builder,
		Action<DiskStorageOptions>? setup,
		string? name = null,
		HealthStatus? failureStatus = null,
		IEnumerable<string>? tags = null,
		TimeSpan? timeout = null)
	{
		var options = new DiskStorageOptions();
		setup?.Invoke(options);

		return builder.Add(new HealthCheckRegistration(name ?? "diskstorage",
			sp =>
				new DiskStorageHealthCheck(options,
					sp.GetRequiredService<IWebLogger>()), failureStatus, tags, timeout));
	}
}
