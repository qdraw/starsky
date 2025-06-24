using System;
using System.Threading.Tasks;
using starsky.feature.packagetelemetry.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks;

public class FakeLifetimeDiagnosticsService : ILifetimeDiagnosticsService
{
	private DateTime StartTime { get; set; }
	
	public Task<DiagnosticsItem?> AddOrUpdateApplicationStopping(DateTime startTime)
	{
		StartTime = startTime;
		return Task.FromResult<DiagnosticsItem?>(new DiagnosticsItem());
	}

	public Task<double> GetLastApplicationStoppingTimeInMinutes()
	{
		var uptime = DateTime.UtcNow - StartTime;
		var minutes = Math.Round(uptime.TotalMinutes, 4);
		return Task.FromResult(minutes);
	}
}
