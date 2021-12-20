using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Models;
using starsky.foundation.sync.WatcherBackgroundService;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.sync.WatcherHelpers
{
	public class QueueProcessor
	{
		private readonly IDiskWatcherBackgroundTaskQueue _bgTaskQueue;
		private readonly SynchronizeDelegate _processFile;

		public QueueProcessor(IServiceScopeFactory serviceProvider, SynchronizeDelegate processFile)
		{
			_bgTaskQueue = serviceProvider.CreateScope().ServiceProvider.GetService<IDiskWatcherBackgroundTaskQueue>();
			_processFile = processFile;
		}

		public QueueProcessor(IDiskWatcherBackgroundTaskQueue bgTaskQueue,
			SynchronizeDelegate processFile)
		{
			_bgTaskQueue = bgTaskQueue;
			_processFile = processFile;
		}

		public delegate Task<List<FileIndexItem>> SynchronizeDelegate(Tuple<string, string, WatcherChangeTypes> value);

		public void QueueInput(string filepath, string toPath,  WatcherChangeTypes changeTypes)
		{
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				await _processFile.Invoke(new Tuple<string, string, WatcherChangeTypes>(filepath,toPath,changeTypes));
			});
		}
	}
}
