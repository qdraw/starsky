using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using starsky.foundation.sync.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.sync.Helpers
{
	/// <summary>
	/// @see: http://web.archive.org/web/20120814142626/http://csharp-codesamples.com/2009/02/file-system-watcher-and-large-file-volumes/
	/// </summary>
	public class FileProcessor
	{
		private readonly Queue<string> _workQueue;
		private Thread _workerThread;
		private readonly IAutoResetEventAsync _waitHandle;
		private readonly FileProcessorDelegate _processFile;
		public delegate void FileProcessorDelegate(string filepath);
		
		public FileProcessor(IAutoResetEventAsync autoResetEventAsync, FileProcessorDelegate processFile)
		{
			_workQueue = new Queue<string>();
			_waitHandle = autoResetEventAsync;
			_processFile = processFile;
		}

		public void QueueInput(string filepath)
		{
			_workQueue.Enqueue(filepath);

			// Initialize and start thread when first file is added
			if (_workerThread == null)
			{
				_workerThread = new Thread(Work);
				_workerThread.Start();
			}
			else if (_workerThread.ThreadState == ThreadState.WaitSleepJoin)
			{
				// If thread is waiting then start it
				_waitHandle.Set();
			}
		}

		internal async void Work()
		{
			var running = true;
			while (running)
			{
				var filepath = RetrieveFile();
				if ( filepath != null )
				{
					_processFile.DynamicInvoke(filepath);
					continue;
				}
				
				// If no files left to process then wait
				running = await _waitHandle.WaitAsync(TimeSpan.FromMinutes(1));
			}
		}

		private string RetrieveFile()
		{
			return _workQueue.Count > 0 ? _workQueue.Dequeue() : null;
		}

	}
}
