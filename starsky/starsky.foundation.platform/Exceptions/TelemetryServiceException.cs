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
			: base(info, context)
		{
		}
	}
}
