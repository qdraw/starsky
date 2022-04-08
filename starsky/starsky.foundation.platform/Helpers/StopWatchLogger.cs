using System;
using System.Diagnostics;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.platform.Helpers
{
	public class StopWatchLogger
	{
		private readonly IWebLogger _logger;

		public StopWatchLogger(IWebLogger logger)
		{
			_logger = logger;
		}
		
		public static Stopwatch StartUpdateReplaceStopWatch()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			return stopWatch;
		}

		public void StopUpdateReplaceStopWatch(string name, string f, bool collections, Stopwatch stopwatch)
		{
			// for debug
			stopwatch.Stop();
			_logger.LogInformation($"[{name}] f: {f} Stopwatch response collections: " +
			                       $"{collections} {DateTime.UtcNow} duration: {stopwatch.Elapsed.TotalMilliseconds} ms or:" +
			                       $" {stopwatch.Elapsed.TotalSeconds} sec");
		}
	}
	
}

