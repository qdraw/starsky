using System;

namespace starsky.foundation.platform.Interfaces
{
	public interface IWebLogger
	{
		void LogTrace(string message, params object[] args);
		void LogInformation(string message, params object[] args);
		void LogInformation(Exception exception, string message,
			params object[] args);

		void LogError(string message, params object[] args);

		void LogError(Exception exception, string message,
			params object[] args);
	}
}
