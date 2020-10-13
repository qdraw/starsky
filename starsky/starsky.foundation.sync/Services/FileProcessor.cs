using System;
using System.Collections.Generic;
using System.Threading;
using starsky.foundation.sync.Interfaces;

namespace starsky.foundation.sync.Services
{
	/// <summary>
	/// @see: http://web.archive.org/web/20120814142626/http://csharp-codesamples.com/2009/02/file-system-watcher-and-large-file-volumes/
	/// </summary>
	public class FileProcessor : IFileProcessor
	{
		private readonly Queue<string> _workQueue;
		private Thread _workerThread;
		private readonly EventWaitHandle _waitHandle;

		public FileProcessor()
		{
			_workQueue = new Queue<string>();
			_waitHandle = new AutoResetEvent(true);
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

			// If thread is waiting then start it
			else if (_workerThread.ThreadState == ThreadState.WaitSleepJoin)
			{
				_waitHandle.Set();
			}
		}

		private void Work()
		{
			while (true)
			{
				var filepath = RetrieveFile();
				if (filepath != null)
					ProcessFile(filepath);
				else
					// If no files left to process then wait
					_waitHandle.WaitOne();
			}

			// ReSharper disable once FunctionNeverReturns
		}

		private string RetrieveFile()
		{
			return _workQueue.Count > 0 ? _workQueue.Dequeue() : null;
		}

		private void ProcessFile(string filepath)
		{
			Console.WriteLine(filepath);
			// Some processing done on the file
			// File.Encrypt(filepath);
		}
	}
}
