using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Exceptions;

namespace starskytest.starsky.foundation.platform.Exceptions
{
	[TestClass]
	public class TelemetryServiceExceptionTest
	{
		[TestMethod]
		[ExpectedException(typeof(TelemetryServiceException))]
		public void TelemetryServiceException()
		{
			var info = new SerializationInfo(typeof(Exception),
				new FormatterConverter());
			info.AddValue("Number", 1);
			info.AddValue("SqlState", "SqlState");
			info.AddValue("Message", "");
			info.AddValue("InnerException", new Exception());
			info.AddValue("HelpURL", "");
			info.AddValue("StackTraceString", "");
			info.AddValue("RemoteStackTraceString", "");
			info.AddValue("RemoteStackIndex", 1);
			info.AddValue("HResult", 1);
			info.AddValue("Source", "");
			info.AddValue("WatsonBuckets",  Array.Empty<byte>());
			
			var ctor =
				typeof(TelemetryServiceException).GetConstructors(BindingFlags.Instance |
					BindingFlags.NonPublic | BindingFlags.InvokeMethod).FirstOrDefault();
			var instance =
				( TelemetryServiceException ) ctor!.Invoke(new object[]
				{
					info,
					new StreamingContext(StreamingContextStates.All)
				});

			throw instance;
		}
	}
}
