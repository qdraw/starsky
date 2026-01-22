using System;
using System.Collections.Generic;
using starsky.foundation.platform.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIWebLogger : IWebLogger
{
	public List<(Exception?, string?)> TrackedExceptions { get; set; } =
		[];

	public List<(Exception?, string?)> TrackedInformation { get; set; } =
		[];

	public List<(Exception?, string?)> TrackedDebug { get; set; } =
		[];

	private readonly object _lock = new();

	public void LogDebug(string? message, params object[] args)
	{
		if ( string.IsNullOrWhiteSpace(message) )
		{
			return;
		}

		try
		{
			Console.WriteLine(message, args);
		}
		catch ( FormatException e )
		{
			Console.WriteLine(e);
		}

		lock ( _lock )
		{
			TrackedDebug.Add(( null, message ));
		}
	}

	public void LogInformation(string? message, params object[] args)
	{
		lock ( _lock )
		{
			TrackedInformation.Add(( null!, message! ));
		}

		try
		{
			Console.WriteLine(message!, args);
		}
		catch ( Exception e )
		{
			Console.WriteLine(e);
		}
	}

	public void LogInformation(Exception exception, string message, params object[] args)
	{
		lock ( _lock )
		{
			TrackedInformation.Add(( exception, message ));
		}

		var exceptionMessage = exception.Message.Replace("{", "{}").Replace("}", "{}");
		Console.WriteLine(exceptionMessage + message, args);
	}

	public void LogError(string? message, params object[] args)
	{
		lock ( _lock )
		{
			TrackedExceptions.Add(( null, message! ));
		}

		Console.WriteLine(message!, args);
	}

	public void LogError(Exception exception, string message, params object[] args)
	{
		lock ( _lock )
		{
			TrackedExceptions.Add(( exception, message ));
		}

		Console.WriteLine(exception.Message + message, args);
	}
}
