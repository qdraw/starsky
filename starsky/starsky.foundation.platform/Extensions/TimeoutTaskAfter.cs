using System;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.platform.Extensions
{
	public static class TimeoutTaskAfter
	{
		public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task,
			int timeout)
		{
			return task.TimeoutAfter(TimeSpan.FromMilliseconds(timeout));
		}

		/// <summary>
		/// @see: https://stackoverflow.com/a/22078975/8613589
		/// </summary>
		/// <param name="task"></param>
		/// <param name="timeout"></param>
		/// <typeparam name="TResult"></typeparam>
		/// <returns></returns>
		/// <exception cref="TimeoutException"></exception>
		public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
		{
			if ( timeout <= TimeSpan.Zero ) throw new TimeoutException("timeout less than 0");
			
			using (var timeoutCancellationTokenSource = new CancellationTokenSource()) {

				var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
				if ( completedTask != task )
					throw new TimeoutException("The operation has timed out.");
				
				timeoutCancellationTokenSource.Cancel();
				return await task;  // Very important in order to propagate exceptions
			}
		}
	}
}
