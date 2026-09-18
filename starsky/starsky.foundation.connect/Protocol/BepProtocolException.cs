using System;

namespace starsky.foundation.connect.Protocol;

/// <summary>
/// Thrown when the BEP v1 protocol is violated (e.g. message too large, bad magic, invalid framing).
/// </summary>
public sealed class BepProtocolException : Exception
{
	public BepProtocolException(string message) : base(message)
	{
	}

	public BepProtocolException(string message, Exception inner) : base(message, inner)
	{
	}
}
