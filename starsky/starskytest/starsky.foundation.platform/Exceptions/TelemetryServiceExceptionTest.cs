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
#pragma warning disable SYSLIB0050
			var info = new SerializationInfo(typeof(Exception),
				new FormatterConverter());
#pragma warning restore SYSLIB0050

			info.AddValue("Message", "");
			info.AddValue("InnerException", new Exception());
			info.AddValue("HelpURL", "");
			info.AddValue("StackTraceString", "");
			info.AddValue("RemoteStackTraceString", "");
			info.AddValue("HResult", 1);
			info.AddValue("Source", "");

			var ctor =
				typeof(TelemetryServiceException).GetConstructors(BindingFlags.Instance |
					BindingFlags.NonPublic | BindingFlags.InvokeMethod).FirstOrDefault();
			var instance =
				( TelemetryServiceException )ctor!.Invoke(new object[]
				{
					info,
#pragma warning disable SYSLIB0050
					new StreamingContext(StreamingContextStates.All)
#pragma warning restore SYSLIB0050
				});

			throw instance;
		}
	}
}
