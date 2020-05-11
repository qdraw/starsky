using System;

namespace starsky.foundation.platform.Exceptions
{
	public class TelemetryServiceException : Exception
	{
		public TelemetryServiceException(string message)
			: base(message)
		{
		}
	}
}
