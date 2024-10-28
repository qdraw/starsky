using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using starsky.feature.health.HealthCheck.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.project.web.ViewModels;

namespace starsky.feature.health.HealthCheck.Service;

[Service(typeof(ICheckHealthService), InjectionLifetime = InjectionLifetime.Scoped)]
public class CheckHealthService : ICheckHealthService
{
	private readonly IMemoryCache? _cache;
	private readonly IWebLogger _logger;
	private readonly HealthCheckService _service;

	public CheckHealthService(HealthCheckService service, IWebLogger logger,
		IMemoryCache? memoryCache = null)
	{
		_service = service;
		_logger = logger;
		_cache = memoryCache;
	}

	public HealthView CreateHealthEntryLog(HealthReport result)
	{
		var health = new HealthView
		{
			IsHealthy = result.Status == HealthStatus.Healthy,
			TotalDuration = result.TotalDuration
		};

		foreach ( var (key, value) in result.Entries )
		{
			health.Entries.Add(
				new HealthEntry
				{
					Duration = value.Duration,
					Name = key,
					IsHealthy = value.Status == HealthStatus.Healthy,
					Description = value.Description ?? string.Empty
				}
			);

			if ( value.Status != HealthStatus.Healthy )
			{
				_logger.LogError($"HealthCheck {key} failed {value.Description} " +
				                 $"{value.Exception?.Message} {value.Exception?.StackTrace}");
			}
		}

		return health;
	}

	/// <summary>
	///     With timeout after 15 seconds
	/// </summary>
	/// <param name="timeoutTime">in milliseconds, defaults to 15 seconds</param>
	/// <returns>report</returns>
	public async Task<HealthReport> CheckHealthWithTimeoutAsync(int timeoutTime = 15000)
	{
		const string healthControllerCacheKey = "health";
		try
		{
			if ( _cache != null &&
			     _cache.TryGetValue(healthControllerCacheKey, out var objectHealthStatus) &&
			     objectHealthStatus is HealthReport healthStatus &&
			     healthStatus.Status == HealthStatus.Healthy )
			{
				return healthStatus;
			}

			var result = await _service.CheckHealthAsync().TimeoutAfter(timeoutTime);
			if ( _cache != null && result.Status == HealthStatus.Healthy )
			{
				_cache.Set(healthControllerCacheKey, result, new TimeSpan(0, 1, 30));
			}

			return result;
		}
		catch ( TimeoutException exception )
		{
			var entry = new HealthReportEntry(
				HealthStatus.Unhealthy,
				"timeout",
				TimeSpan.FromMilliseconds(timeoutTime),
				exception,
				null);

			return new HealthReport(
				new Dictionary<string, HealthReportEntry> { { "timeout", entry } },
				TimeSpan.FromMilliseconds(timeoutTime));
		}
	}
}
