using System;
using System.Collections.Generic;
using starsky.foundation.platform.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIWebLogger : IWebLogger
	{

		public List<(Exception, string)> TrackedExceptions { get; set; } =
			new List<(Exception, string)>();
		public List<(Exception, string)> TrackedInformation { get; set; } =
			new List<(Exception, string)>();
		public List<(Exception, string)> TrackedDebug { get; set; } =
			new List<(Exception, string)>();
		
		public void LogDebug(string message, params object[] args)
		{
			TrackedDebug.Add((null, message));
			Console.WriteLine(message, args);
		}

		public void LogInformation(string message, params object[] args)
		{
			TrackedInformation.Add((null, message));
			try
			{
				Console.WriteLine(message, args);
			}
			catch ( Exception e )
			{
				// ignored
			}
		}

		public void LogInformation(Exception exception, string message, params object[] args)
		{
			TrackedInformation.Add((exception, message));
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
