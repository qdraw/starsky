using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Services
{
	[SuppressMessage("Usage", "CA2254:The logging message template should not vary between calls to " +
							  "'LoggerExtensions.LogInformation(ILogger, string?, params object?[])'")]
	[Service(typeof(IWebLogger), InjectionLifetime = InjectionLifetime.Singleton)]
	public sealed class WebLogger : IWebLogger
	{
		private readonly ILogger? _logger;
		private readonly IConsole? _console;
		private readonly AppSettings? _appSettings;

		/// <summary>
		/// Trace = 0, Debug = 1, Information = 2, Warning = 3, Error = 4, Critical = 5, and None = 6.
		/// @see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0
		/// </summary>
		/// <param name="loggerFactory">ILoggerFactory</param>
		/// <param name="scopeFactory">optional scopeFactory</param>
		public WebLogger(ILoggerFactory? loggerFactory = null, IServiceScopeFactory? scopeFactory = null)
		{
			_logger = loggerFactory?.CreateLogger("app");
			var scopeProvider = scopeFactory?.CreateScope().ServiceProvider;
			if ( scopeProvider == null ) return;
			_console = scopeProvider.GetService<IConsole>();
			_appSettings = scopeProvider.GetService<AppSettings>();
		}

		public void LogDebug(string? message, params object[] args)
		{
			if ( string.IsNullOrWhiteSpace(message) )
			{
				return;
			}

			if ( _logger == null && _appSettings?.Verbose == true )
			{
				_console?.WriteLine(message);
				return;
			}
			
			_logger?.LogDebug(message, args);
		}

		public void LogInformation(string? message, params object[] args)
		{
			if ( string.IsNullOrWhiteSpace(message) )
			{
				return;
			}

			if ( _logger == null )
			{
				_console?.WriteLine(message);
				return;
			}
			_logger.LogInformation(message, args);
		}

		public void LogInformation(Exception exception, string message,
			params object[] args)
		{
			if ( _logger == null )
			{
				_console?.WriteLine($"{exception.Message} {message}");
				return;
			}
			_logger.LogInformation(exception, message, args);
		}

		public void LogError(string? message, params object[] args)
		{
			if ( string.IsNullOrWhiteSpace(message) )
			{
				return;
			}

			if ( _logger == null )
			{
				_console?.WriteLine(message);
				return;
			}
			_logger.LogError(message, args);
		}
		public void LogError(Exception exception, string message, params object[] args)
		{
			if ( _logger == null )
			{
				_console?.WriteLine($"{exception.Message} {message}");
				return;
			}
			_logger.LogError(exception, message, args);
		}
	}
}
