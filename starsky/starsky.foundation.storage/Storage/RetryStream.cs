using System;
using System.IO;
using System.Threading;

namespace starsky.foundation.storage.Storage
{
	public class RetryStream
	{
		public delegate Stream RetryStreamDelegate();

		public Stream Retry(RetryStreamDelegate LocalGet)
		{
			var maxRetry = 3;
			for ( int retry = 0; retry < maxRetry; retry++ )
			{
				try
				{
					return LocalGet();
				}
				catch ( IOException error )
				{
					if ( retry < maxRetry - 1 )
					{
						Console.WriteLine($"catched > {retry}/{maxRetry} {error}");
						Thread.Sleep(2000);
						continue;
					}
					Console.WriteLine("FAIL > " + error);
				}
			}
			return Stream.Null;
		}
	}
}
