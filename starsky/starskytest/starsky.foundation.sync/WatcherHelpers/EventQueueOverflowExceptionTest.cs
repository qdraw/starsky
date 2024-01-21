using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.sync.WatcherHelpers;

namespace starskytest.starsky.foundation.sync.WatcherHelpers
{
	[TestClass]
	public sealed class EventQueueOverflowExceptionTest
	{
		[TestMethod]
		public void EventQueueOverflowException()
		{
			var exception =  new EventQueueOverflowException();
			Assert.AreEqual($"Exception of type '{typeof(EventQueueOverflowException)}' was thrown.", exception.Message);
		}
		
		[TestMethod]
		public void EventQueueOverflowExceptionMessage()
		{
			var exception =  new EventQueueOverflowException("message");
			Assert.AreEqual("message", exception.Message);
		}
		
		[TestMethod]
		[ExpectedException(typeof(EventQueueOverflowException))]
		public void EventQueueOverflowException_Protected()
		{
#pragma warning disable SYSLIB0050
			var info = new SerializationInfo(typeof(Exception),
				new FormatterConverter());
			info.AddValue("Message", "");
			info.AddValue("InnerException", new Exception());
			info.AddValue("HelpURL", "");
			info.AddValue("StackTraceString", "");
			info.AddValue("RemoteStackTraceString", "");
			info.AddValue("HResult", 1);
			info.AddValue("Source", "");
			
			var ctor =
				typeof(EventQueueOverflowException).GetConstructors(BindingFlags.Instance |
					BindingFlags.NonPublic | BindingFlags.InvokeMethod).FirstOrDefault();
			if ( ctor == null ) throw new NullReferenceException();
			var instance =
				( EventQueueOverflowException ) ctor.Invoke(new object[]
				{
					info,
					new StreamingContext(StreamingContextStates.All)
				});

			throw instance;
#pragma warning restore SYSLIB0050
		}
	}
}
