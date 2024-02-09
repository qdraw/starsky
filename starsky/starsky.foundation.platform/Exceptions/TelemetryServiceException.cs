using System;
using System.Runtime.Serialization;

namespace starsky.foundation.platform.Exceptions
{
	[Serializable]
	public class TelemetryServiceException : Exception
	{
		public TelemetryServiceException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Without this constructor, deserialization will fail
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected TelemetryServiceException(SerializationInfo info, StreamingContext context)
#pragma warning disable SYSLIB0051
			: base(info, context)
#pragma warning restore SYSLIB0051
		{
		}
	}
}
