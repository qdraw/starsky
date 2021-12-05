using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.sync.WatcherHelpers
{
	/// <summary>
	/// Service is created only once, and used everywhere
	/// @see: http://web.archive.org/web/20120814142626/http://csharp-codesamples.com/2009/02/file-system-watcher-and-large-file-volumes/
	/// </summary>
	[Obsolete("use QueueProcessor")]
	public class FileProcessor
	{
		private readonly Queue<Tuple<string, string, WatcherChangeTypes>> _workQueue;
		private Thread _workerThread;
		private readonly SynchronizeDelegate _processFile;
		private readonly AutoResetEvent _waitHandle;

		public delegate Task<List<FileIndexItem>> SynchronizeDelegate(Tuple<string, string, WatcherChangeTypes> value);
		
		public FileProcessor(SynchronizeDelegate processFile)
		{
			_workQueue = new Queue<Tuple<string, string, WatcherChangeTypes>>();
			_waitHandle =  new AutoResetEvent(true);
			_processFile = processFile;
		}

		public void QueueInput(string filepath, string toPath,  WatcherChangeTypes changeTypes)
		{
			var item = new Tuple<string, string, WatcherChangeTypes>(filepath, toPath, changeTypes);
			_workQueue.Enqueue(item);

			// Initialize and start thread when first file is added
			if (_workerThread == null)
			{
				_workerThread = new Thread(EndlessWorkQueue);
				_workerThread.Start();
			}
			// https://docs.microsoft.com/en-us/dotnet/api/system.threading.threadstate?view=net-5.0
			else switch ( _workerThread.ThreadState )
			{
				case ThreadState.WaitSleepJoin:
					// If thread is waiting then start it
					_waitHandle.Set();
					break;
				case ThreadState.Stopped:
					_waitHandle.Set();
					break;
			}
		}

		private void EndlessWorkQueue()
		{
			EndlessWorkQueueAsync(CancellationToken.None).GetAwaiter().GetResult();
		}

		[SuppressMessage("ReSharper", "FunctionNeverReturns")]
		[SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter")]
		internal async Task EndlessWorkQueueAsync(CancellationToken token)
		{
			while ( true )
			{
				var retrieveFileObject = RetrieveFile();

				if ( retrieveFileObject != null)
				{
					await _processFile.Invoke(retrieveFileObject);
					continue;
				}
				
				if ( token.IsCancellationRequested )
				{
					return;
				}
				// If no files left to process then wait
				_waitHandle.WaitOne();
			}
		}

		private Tuple<string, string, WatcherChangeTypes> RetrieveFile()
		{
			return _workQueue.Count > 0 ? _workQueue.Dequeue() : null;
		}

	}
}
