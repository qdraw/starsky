using System;
using Microsoft.Extensions.Logging;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.platform.Services
{
	[Service(typeof(IWebLogger), InjectionLifetime = InjectionLifetime.Scoped)]
	public class WebLogger : IWebLogger
	{
		private readonly ILogger _logger;
		private readonly IConsole _console;

		public WebLogger(IConsole console, ILoggerFactory logger = null)
		{
			_console = console;
			_logger = logger?.CreateLogger("app");
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
				_console.WriteLine(message);
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
				_console.WriteLine(message);
				return;
			}
			_logger.LogError(exception, message,args);
		}
		
	}
}
