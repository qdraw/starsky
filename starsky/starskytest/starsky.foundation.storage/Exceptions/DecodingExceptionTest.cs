using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Exceptions;

namespace starskytest.starsky.foundation.storage.Exceptions;

[TestClass]
public sealed class DecodingExceptionTest
{
	[TestMethod]
	public void DecodingException()
	{
		// Arrange
#pragma warning disable SYSLIB0050
		var info = new SerializationInfo(typeof(Exception), new FormatterConverter());
		info.AddValue("Message", "");
		info.AddValue("InnerException", new Exception());
		info.AddValue("HelpURL", "");
		info.AddValue("StackTraceString", "");
		info.AddValue("RemoteStackTraceString", "");
		info.AddValue("HResult", 1);
		info.AddValue("Source", "");

		var ctor = typeof(DecodingException).GetConstructors(BindingFlags.Instance |
		                                                     BindingFlags.NonPublic)
			.FirstOrDefault();

		if ( ctor == null )
		{
			Assert.Fail("No suitable constructor found for DecodingException.");
		}

		// Act & Assert
		var ex = Assert.ThrowsException<DecodingException>(() =>
		{
			var instance = ( DecodingException ) ctor.Invoke(new object[]
			{
				info, new StreamingContext(StreamingContextStates.All)
			});
			throw instance;
		});
#pragma warning restore SYSLIB0050

		// Optionally verify the exception message or other properties
		Assert.IsTrue(ex.ToString().Contains("System.Exception"));
	}
}
