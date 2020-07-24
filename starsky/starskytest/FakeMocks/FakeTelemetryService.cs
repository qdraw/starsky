using System;
using System.Collections.Generic;
using starsky.foundation.platform.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeTelemetryService : ITelemetryService
	{
		public List<Exception> TrackedExceptions { get; set; } = new List<Exception>();
		
		public bool TrackException(Exception exception)
		{
			TrackedExceptions.Add(exception);
			return true;
		}
	}
}
