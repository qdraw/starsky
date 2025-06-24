using System;
using System.Globalization;
using System.Threading.Tasks;
using starsky.feature.packagetelemetry.Interfaces;
using starsky.foundation.database.Diagnostics;
using starsky.foundation.database.Diagnostics.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;

namespace starsky.feature.packagetelemetry.Services;

[Service(typeof(ILifetimeDiagnosticsService), InjectionLifetime = InjectionLifetime.Scoped)]
public class LifetimeDiagnosticsService(IDiagnosticsService diagnosticsService, IWebLogger logger)
	: ILifetimeDiagnosticsService
{
	public async Task<DiagnosticsItem?> AddOrUpdateApplicationStopping(
		DateTime startTime)
	{
		var uptime = DateTime.UtcNow - startTime;
		var minutes = Math.Round(uptime.TotalMinutes, 4);

		logger.LogInformation(
			$"[LifetimeDiagnosticsService] Application stopping lifetime in minutes: {minutes}");
		var item = await diagnosticsService.AddOrUpdateItem(
			DiagnosticsType.ApplicationStoppingLifetimeInMinutes,
			minutes.ToString(CultureInfo.InvariantCulture));
		return item;
	}

	public async Task<double> GetLastApplicationStoppingTimeInMinutes()
	{
		var item = await diagnosticsService.GetItem(
			DiagnosticsType.ApplicationStoppingLifetimeInMinutes);

		if ( !double.TryParse(item?.Value, NumberStyles.Any,
			    CultureInfo.InvariantCulture, out var result) )
		{
			return -1;
		}

		return result;
	}
}
