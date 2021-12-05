using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Models;
using starsky.foundation.sync.WatcherBackgroundService;

namespace starsky.foundation.sync.WatcherHelpers
{
	public class QueueProcessor
	{
		private readonly DiskWatcherBackgroundTaskQueue _bgTaskQueue;
		private readonly SynchronizeDelegate _processFile;
		private readonly IServiceScopeFactory _scopeFactory;

		public QueueProcessor(IServiceScopeFactory scopeFactory, SynchronizeDelegate processFile)
		{
			_scopeFactory = scopeFactory;
			_processFile = processFile;
		}

		public delegate Task<List<FileIndexItem>> SynchronizeDelegate(Tuple<string, string, WatcherChangeTypes> value);

		public void QueueInput(string filepath, string toPath,  WatcherChangeTypes changeTypes)
		{
			var bgTaskQueue = _scopeFactory.CreateScope().ServiceProvider
				.GetService<DiskWatcherBackgroundTaskQueue>();
			
			bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				await _processFile.Invoke(new Tuple<string, string, WatcherChangeTypes>(filepath,toPath,changeTypes));
			});
		}
	}
}
