using System;
using System.Collections.Generic;
using starsky.foundation.platform.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIWebLogger : IWebLogger
	{

		public List<(Exception, string)> TrackedExceptions { get; set; } =
			new List<(Exception, string)>();
		
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
			TrackedExceptions.Add((null, message));
			Console.WriteLine(message, args);
		}

		public void LogError(Exception exception, string message, params object[] args)
		{
			TrackedExceptions.Add((exception, message));
			Console.WriteLine(exception.Message + message, args);
		}
	}
}
