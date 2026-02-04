using System;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.packagetelemetry.Interfaces;

public interface ILifetimeDiagnosticsService
{
	Task<DiagnosticsItem?> AddOrUpdateApplicationStopping(DateTime startTime);
	Task<double> GetLastApplicationStoppingTimeInMinutes();
}
