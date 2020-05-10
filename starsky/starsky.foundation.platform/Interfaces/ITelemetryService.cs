using System;

namespace starsky.foundation.platform.Interfaces
{
	public interface ITelemetryService
	{
		bool TrackException(Exception exception);
	}
}
