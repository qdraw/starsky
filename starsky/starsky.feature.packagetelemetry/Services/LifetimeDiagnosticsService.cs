using System;
using System.Globalization;
using System.Threading.Tasks;
using starsky.feature.packagetelemetry.Interfaces;
using starsky.foundation.database.Diagnostics;
using starsky.foundation.database.Diagnostics.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;

namespace starsky.feature.packagetelemetry.Services;

[Service(typeof(ILifetimeDiagnosticsService), InjectionLifetime = InjectionLifetime.Scoped)]
public class LifetimeDiagnosticsService(IDiagnosticsService diagnosticsService)
	: ILifetimeDiagnosticsService
{
	public async Task<DiagnosticsItem?> AddOrUpdateApplicationStopping(
		DateTime startTime)
	{
		var uptime = DateTime.UtcNow - startTime;
		var minutes = Math.Round(uptime.TotalMinutes, 4);
		
		var item = await diagnosticsService.AddOrUpdateItem(
			DiagnosticsType.ApplicationStoppingLifetimeInMinutes,
			minutes.ToString(CultureInfo.InvariantCulture));
		return item;
	}
}
