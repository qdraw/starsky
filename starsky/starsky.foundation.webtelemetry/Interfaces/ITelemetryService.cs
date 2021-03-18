using System;

namespace starsky.foundation.webtelemetry.Interfaces
{
	public interface ITelemetryService
	{
		bool TrackException(Exception exception);
	}
}
