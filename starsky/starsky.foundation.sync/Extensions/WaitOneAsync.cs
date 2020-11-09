using System;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.sync.Extensions
{
	public static class WaitOneAsyncExtension
	{
		/// <summary>
		/// @see: https://thomaslevesque.com/2015/06/04/async-and-cancellation-support-for-wait-handles/
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="millisecondsTimeout"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			RegisteredWaitHandle registeredHandle = null;
			CancellationTokenRegistration tokenRegistration = default(CancellationTokenRegistration);
			try
			{
				var tcs = new TaskCompletionSource<bool>();
				registeredHandle = ThreadPool.RegisterWaitForSingleObject(
					handle,
					(state, timedOut) => ((TaskCompletionSource<bool>)state)?.TrySetResult(!timedOut),
					tcs,
					millisecondsTimeout,
					true);
				tokenRegistration = cancellationToken.Register(
					state => ((TaskCompletionSource<bool>)state).TrySetCanceled(),
					tcs);
				return await tcs.Task;
			}
			finally
			{
				if (registeredHandle != null)
					registeredHandle.Unregister(null);
				tokenRegistration.Dispose();
			}
		}

		public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
		{
			return handle.WaitOneAsync((int)timeout.TotalMilliseconds, cancellationToken);
		}

		public static Task<bool> WaitOneAsync(this WaitHandle handle, CancellationToken cancellationToken)
		{
			return handle.WaitOneAsync(Timeout.Infinite, cancellationToken);
		}

	}
}
