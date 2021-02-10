using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.platform.Helpers
{
	/// <summary>
	/// @see: https://stackoverflow.com/a/1563234
	/// @see: https://alastaircrabtree.com/implementing-the-retry-pattern-for-async-tasks-in-c/
	/// </summary>
	public static class RetryHelper
	{
		public static T Do<T>(
			Func<T> action,
			TimeSpan retryInterval,
			int maxAttemptCount = 3)
		{
			if (maxAttemptCount <= 0) 
				throw new ArgumentOutOfRangeException(nameof(maxAttemptCount));
			
			var exceptions = new List<Exception>();

			for (int attempted = 0; attempted < maxAttemptCount; attempted++)
			{
				try
				{
					if (attempted > 0)
					{
						Thread.Sleep(retryInterval);
					}
					return action();
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}
			throw new AggregateException(exceptions);
		}
		
		public static Task<T> DoAsync<T>(
			  Func<Task<T>> operation, TimeSpan delay, int maxAttemptCount = 3 )
		{
			if (maxAttemptCount <= 0) 
				throw new ArgumentOutOfRangeException(nameof(maxAttemptCount));

			return DoAsyncWorker(operation, delay, maxAttemptCount);
		}

		private static async Task<T> DoAsyncWorker<T>(
			Func<Task<T>> operation, TimeSpan delay, int maxAttemptCount = 3)
		{
			var exceptions = new List<Exception>();
			for ( int attempted = 0; attempted < maxAttemptCount; attempted++ )
			{
				try
				{
					if (attempted > 0)
					{
						await Task.Delay(delay);
					}
					return await operation();
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}
			throw new AggregateException(exceptions);
		}
	}
}
