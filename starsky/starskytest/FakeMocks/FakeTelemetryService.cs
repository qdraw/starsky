using System;
using starsky.foundation.platform.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeTelemetryService : ITelemetryService
	{
		public bool TrackException(Exception exception)
		{
			// do nothing
			return true;
		}
	}
}
