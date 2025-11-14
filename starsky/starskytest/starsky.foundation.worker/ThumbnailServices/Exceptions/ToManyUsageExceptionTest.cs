using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.worker.ThumbnailServices.Exceptions;

namespace starskytest.starsky.foundation.worker.ThumbnailServices.Exceptions;

[TestClass]
public sealed class ToManyUsageExceptionTest
{
	[TestMethod]
	public void ToManyUsageException_DefaultConstructor()
	{
		var exception = new ToManyUsageException();
		Assert.AreEqual($"Exception of type '{typeof(ToManyUsageException)}' was thrown.",
			exception.Message);
	}

	[TestMethod]
	public void ToManyUsageExceptionMessage()
	{
		var exception = new ToManyUsageException("message");
		Assert.AreEqual("message", exception.Message);
	}

	[TestMethod]
	public void ToManyUsageException_Protected()
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
			typeof(ToManyUsageException).GetConstructor(
				BindingFlags.Instance | BindingFlags.NonPublic,
				null,
				new[] { typeof(SerializationInfo), typeof(StreamingContext) },
				null);
		if ( ctor == null )
		{
			throw new NullReferenceException();
		}

		var instance =
			( ToManyUsageException ) ctor.Invoke(new object[]
			{
				info, new StreamingContext(StreamingContextStates.All)
			});

#pragma warning disable MSTEST0039
		try
		{
			throw instance;
		}
		catch (ToManyUsageException ex)
		{
			Assert.IsInstanceOfType(ex, typeof(ToManyUsageException));
		}
#pragma warning restore MSTEST0039
#pragma warning restore SYSLIB0050
	}
}
