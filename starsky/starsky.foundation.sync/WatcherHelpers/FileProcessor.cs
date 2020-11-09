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


		private void EndlessWorkQueue()
		{
			EndlessWorkQueue(true);
		}

		[SuppressMessage("ReSharper", "FunctionNeverReturns")]
		[SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter")]
		internal async void EndlessWorkQueue(bool enableWaitOne = true)
		{
			while ( true )
			{
				var filepath = RetrieveFile();

				if ( filepath != null )
				{
					await _processFile.Invoke(filepath);
					Console.WriteLine("inv " + filepath);
					continue;
				}

				// If no files left to process then wait
				if ( !enableWaitOne ) return;
				_waitHandle.WaitOne();
			}
		}

		private string RetrieveFile()
		{
			return _workQueue.Count > 0 ? _workQueue.Dequeue() : null;
		}

	}
}
