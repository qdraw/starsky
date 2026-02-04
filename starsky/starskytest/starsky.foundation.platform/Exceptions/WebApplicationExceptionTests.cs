using System;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Exceptions;

namespace starskytest.starsky.foundation.platform.Exceptions;

[TestClass]
public class WebApplicationExceptionTests
{
	[TestMethod]
	public void WebApplicationException_DefaultConstructor_Message()
	{
		// Arrange & Act
		var exception = new WebApplicationException();

		// Assert
		Assert.AreEqual($"Exception of type '{typeof(WebApplicationException)}' was thrown.",
			exception.Message);
	}

	[TestMethod]
	public void WebApplicationException_ConstructorWithMessage_SetsMessageCorrectly()
	{
		// Arrange
		const string message = "Test Message";

		// Act
		var exception = new WebApplicationException(message);

		// Assert
		Assert.AreEqual(message, exception.Message);
	}

	[TestMethod]
	public void
		WebApplicationException_ConstructorWithMessageAndInnerException_SetsMessageAndInnerExceptionCorrectly()
	{
		// Arrange
		const string message = "Test Message";
		var innerException = new Exception("Inner exception message");

		// Act
		var exception = new WebApplicationException(message, innerException);

		// Assert
		Assert.AreEqual(message, exception.Message);
		Assert.AreEqual(innerException, exception.InnerException);
	}

	[TestMethod]
	public void WebApplicationException_SerializationConstructor_SetsPropertiesCorrectly()
	{
		// Arrange
		var message = "Test Message";
		var innerException = new Exception("Inner exception message");
		var serializationInfo =
#pragma warning disable SYSLIB0050
			new SerializationInfo(typeof(WebApplicationException), new FormatterConverter());
#pragma warning restore SYSLIB0050
		serializationInfo.AddValue("Message", message);
		serializationInfo.AddValue("InnerException", innerException);
		serializationInfo.AddValue("HelpURL", "help");
		serializationInfo.AddValue("StackTraceString", "StackTraceString");
		serializationInfo.AddValue("RemoteStackTraceString", "RemoteStackTraceString");
		serializationInfo.AddValue("HResult", 1);
		serializationInfo.AddValue("Source", "");

		var streamingContext = new StreamingContext();
		var constructorInfo = typeof(WebApplicationException).GetConstructor(
			BindingFlags.NonPublic | BindingFlags.Instance, null,
			new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);

		// Act
		var exception =
			( WebApplicationException )constructorInfo!.Invoke(new object[]
			{
				serializationInfo, streamingContext
			});

		// Assert
		Assert.AreEqual(message, exception.Message);
		Assert.AreEqual(innerException, exception.InnerException);
	}
}
