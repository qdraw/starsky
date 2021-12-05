using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.platform.Services
{
	[Service(typeof(IWebLogger), InjectionLifetime = InjectionLifetime.Singleton)]
	public class WebLogger : IWebLogger
	{
		private readonly ILogger _logger;
		private readonly IConsole _console;

		/// <summary>
		/// Trace = 0, Debug = 1, Information = 2, Warning = 3, Error = 4, Critical = 5, and None = 6.
		/// @see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0
		/// </summary>
		/// <param name="logger">ILoggerFactory</param>
		/// <param name="scopeFactory">optional scopeFactory</param>
		public WebLogger(ILoggerFactory logger = null, IServiceScopeFactory scopeFactory = null)
		{
			_logger = logger?.CreateLogger("app");
			var scopeProvider = scopeFactory?.CreateScope().ServiceProvider;
			if ( scopeProvider != null ) _console = scopeProvider.GetService<IConsole>();
		}

		public void LogDebug(string message, params object[] args)
		{
			if ( _logger == null )
			{
				_console.WriteLine(message);
				return;
			}
			_logger.LogDebug(message, args);
		}

		public void LogInformation(string message, params object[] args)
		{
			if ( _logger == null )
			{
				_console.WriteLine(message);
				return;
			}
			_logger.LogInformation(message, args);
		}

		public void LogInformation(Exception exception, string message,
			params object[] args)
		{
			if ( _logger == null )
			{
				_console.WriteLine($"{exception.Message} {message}");
				return;
			}
			_logger.LogInformation(exception, message, args);
		}

		public void LogError(string message, params object[] args)
		{
			if ( _logger == null )
			{
				_console.WriteLine(message);
				return;
			}
			_logger.LogError(message,args);
		}
		public void LogError(Exception exception, string message, params object[] args)
		{
			if ( _logger == null )
			{
				_console.WriteLine($"{exception.Message} {message}");
				return;
			}
			_logger.LogError(exception, message,args);
		}
	}
}
