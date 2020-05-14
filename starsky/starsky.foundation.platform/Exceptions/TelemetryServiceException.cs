using System;

namespace starsky.foundation.platform.Exceptions
{
	[Serializable]
	public class TelemetryServiceException : Exception
	{
		public TelemetryServiceException(string message)
			: base(message)
		{
		}
	}
}
