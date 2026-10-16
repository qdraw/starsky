using System;
using Microsoft.Extensions.Logging;
using Sentry;

namespace starsky.foundation.webtelemetry.Helpers;

/// <summary>
///     Log lines become Sentry breadcrumbs, so a crash report shows what the application was
///     doing before it crashed. Every breadcrumb is scrubbed in <see cref="SetupSentry" />
///     before it is added to the scope.
/// </summary>
internal sealed class SentryBreadcrumbLoggerProvider : ILoggerProvider
{
	public ILogger CreateLogger(string categoryName)
	{
		return new SentryBreadcrumbLogger(categoryName);
	}

	public void Dispose()
	{
		// the logger holds no resources, the Sentry client is disposed by SetupSentry
	}

	private sealed class SentryBreadcrumbLogger(string categoryName) : ILogger
	{
		/// <summary>
		///     Debug and trace lines would push the useful ones out of the buffer
		/// </summary>
		private const LogLevel MinimumLevel = LogLevel.Information;

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			// false keeps the logger from formatting lines that are never used
			return logLevel >= MinimumLevel && SentrySdk.IsEnabled;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
			Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if ( !IsEnabled(logLevel) )
			{
				return;
			}

			var (type, level) = ToBreadcrumb(logLevel);
			SentrySdk.AddBreadcrumb(new Breadcrumb(formatter(state, exception), type,
				category: categoryName, level: level));
		}

		private static (string Type, BreadcrumbLevel Level) ToBreadcrumb(LogLevel logLevel)
		{
			return logLevel switch
			{
				LogLevel.Critical => ( BreadcrumbTypes.Error, BreadcrumbLevel.Fatal ),
				LogLevel.Error => ( BreadcrumbTypes.Error, BreadcrumbLevel.Error ),
				LogLevel.Warning => ( BreadcrumbTypes.Warning, BreadcrumbLevel.Warning ),
				_ => ( BreadcrumbTypes.Info, BreadcrumbLevel.Info )
			};
		}
	}
}
