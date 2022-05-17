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
		/// <summary>
		/// Retry when Exception happens for sync
		/// </summary>
		/// <param name="action">function</param>
		/// <param name="retryInterval">Delay in timespan</param>
		/// <param name="maxAttemptCount">number of tries, must be more that 0</param>
		/// <typeparam name="T">The function</typeparam>
		/// <returns>value of function</returns>
		/// <exception cref="ArgumentOutOfRangeException">when lower or eq than 0</exception>
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
		
		/// <summary>
		/// Retry when Exception happens with the async await pattern
		/// </summary>
		/// <param name="operation">Async function</param>
		/// <param name="delay">Delay in timespan</param>
		/// <param name="maxAttemptCount">number of tries, must be more that 0</param>
		/// <typeparam name="T">The async function</typeparam>
		/// <returns>value of function</returns>
		/// <exception cref="ArgumentOutOfRangeException">when lower or eq than 0</exception>
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
						// Thread.Sleep
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
