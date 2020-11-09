using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
	public class FileProcessor
	{
		private readonly Queue<string> _workQueue;
		private Thread _workerThread;
		private readonly SynchronizeDelegate _processFile;
		private readonly AutoResetEvent _waitHandle;
		public delegate Task<List<FileIndexItem>> SynchronizeDelegate(string filepath, bool recursive = true);
		
		public FileProcessor(SynchronizeDelegate processFile)
		{
			_workQueue = new Queue<string>();
			_waitHandle =  new AutoResetEvent(true);
			_processFile = processFile;
		}

		public void QueueInput(string filepath)
		{
			_workQueue.Enqueue(filepath);

			// Initialize and start thread when first file is added
			if (_workerThread == null)
			{
				_workerThread = new Thread(EndlessWorkQueue);
				_workerThread.Start();
			}
			else if (_workerThread.ThreadState == ThreadState.WaitSleepJoin)
			{
				// If thread is waiting then start it
				_waitHandle.Set();
			}
		}


		internal void EndlessWorkQueue()
		{
			EndlessWorkQueueAsync().ConfigureAwait(false);
		}

		[SuppressMessage("ReSharper", "FunctionNeverReturns")]
		[SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter")]
		private async Task EndlessWorkQueueAsync()
		{
			while ( true )
			{
				var filepath = RetrieveFile();

				if ( filepath != null )
				{
					await _processFile.Invoke(filepath);
					Console.WriteLine("invoked: " + filepath);
					continue;
				}

				// If no files left to process then wait
				_waitHandle.WaitOne();
			}
		}

		private string RetrieveFile()
		{
			return _workQueue.Count > 0 ? _workQueue.Dequeue() : null;
		}

	}
}
