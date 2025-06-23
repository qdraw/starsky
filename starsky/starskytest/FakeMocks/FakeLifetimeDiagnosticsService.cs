using System;
using System.Threading.Tasks;
using starsky.feature.packagetelemetry.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks;

public class FakeLifetimeDiagnosticsService : ILifetimeDiagnosticsService
{
	public Task<DiagnosticsItem?> AddOrUpdateApplicationStopping(DateTime startTime)
	{
		return Task.FromResult<DiagnosticsItem?>(new DiagnosticsItem());
	}

	public Task<double> GetLastApplicationStoppingTimeInMinutes()
	{
		return Task.FromResult<double>(0);
	}
}
