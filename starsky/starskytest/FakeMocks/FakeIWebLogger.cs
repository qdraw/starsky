using System;
using starsky.foundation.platform.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIWebLogger : IWebLogger
	{
		public void LogInformation(string message, params object[] args)
		{
			Console.WriteLine(message, args);
		}

		public void LogInformation(Exception exception, string message, params object[] args)
		{
			Console.WriteLine(exception.Message + message, args);
		}

		public void LogError(string message, params object[] args)
		{
			Console.WriteLine(message, args);
		}

		public void LogError(Exception exception, string message, params object[] args)
		{
			Console.WriteLine(exception.Message + message, args);
		}
	}
}
