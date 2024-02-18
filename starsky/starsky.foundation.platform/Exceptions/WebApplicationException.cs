using System;
using System.Runtime.Serialization;

#pragma warning disable SYSLIB0051

namespace starsky.foundation.platform.Exceptions;

[Serializable]
public class WebApplicationException : Exception
{
	public WebApplicationException()
	{
	}

	public WebApplicationException(string message)
		: base(message)
	{
	}

	public WebApplicationException(string message, System.Exception inner)
		: base(message, inner)
	{
	}

	// Serialization constructor
	protected WebApplicationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		// nothing here
	}
}
