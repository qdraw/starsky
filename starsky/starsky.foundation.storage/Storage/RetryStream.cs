using System;
using System.IO;
using System.Threading;

namespace starsky.foundation.storage.Storage
{
	public class RetryStream
	{
		private readonly int _waitTime;

		public RetryStream(int waitTime = 2000)
		{
			_waitTime = waitTime;
		}
		
		public delegate Stream RetryStreamDelegate();

		/// <summary>
		/// Retry Stream
		/// @see: https://stackoverflow.com/a/26378102/8613589
		/// </summary>
		/// <param name="localGet">Stream RetryStreamDelegate</param>
		/// <returns>Stream</returns>
		public Stream Retry(RetryStreamDelegate localGet)
		{
			const int maxRetry = 3;
			for ( var retry = 0; retry < maxRetry; retry++ )
			{
				try
				{
					return localGet();
				}
				catch ( IOException error )
				{
					if ( retry < maxRetry - 1 )
					{
						Console.WriteLine($"catch-ed > {retry}/{maxRetry-1} {error}");
						Thread.Sleep(_waitTime);
						continue;
					}
					Console.WriteLine("FAIL > " + error);
				}
			}
			return Stream.Null;
		}
	}
}
