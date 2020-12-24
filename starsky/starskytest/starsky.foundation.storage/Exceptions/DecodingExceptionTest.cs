using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Exceptions;

namespace starskytest.starsky.foundation.storage.Exceptions
{
	[TestClass]
	public class DecodingExceptionTest
	{
		[TestMethod]
		[ExpectedException(typeof(DecodingException))]
		public void DecodingException()
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
			info.AddValue("WatsonBuckets", new byte[0]);
			
			var ctor =
				typeof(DecodingException).GetConstructors(BindingFlags.Instance |
					BindingFlags.NonPublic | BindingFlags.InvokeMethod).FirstOrDefault();
			var instance =
				( DecodingException ) ctor.Invoke(new object[]
				{
					info,
					new StreamingContext(StreamingContextStates.All)
				});

			throw instance;
		}
	}
}
