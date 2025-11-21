using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace starskytest.FakeMocks;

public sealed class FakeILogger<T> : ILogger<T>
{
	public List<string> Messages { get; } = new();
	public List<LogLevel> LogLevels { get; } = new();

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
		Exception? exception, Func<TState, Exception?, string> formatter)
	{
		LogLevels.Add(logLevel);
		if ( exception != null )
		{
			Messages.Add(exception.Message);
			return;
		}

		Messages.Add(state?.ToString() ?? string.Empty);
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return true;
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
	{
		return null;
	}
}
