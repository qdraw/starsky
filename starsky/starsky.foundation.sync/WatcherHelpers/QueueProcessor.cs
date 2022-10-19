using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Models;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.sync.WatcherInterfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.sync.WatcherHelpers
{
	public sealed class QueueProcessor : IQueueProcessor // not injected
	{
		private readonly IDiskWatcherBackgroundTaskQueue _bgTaskQueue;
		private readonly SynchronizeDelegate _processFile;

		public QueueProcessor(IServiceScopeFactory serviceProvider,
			SynchronizeDelegate processFile)
		{
			_bgTaskQueue = serviceProvider.CreateScope().ServiceProvider.GetService<IDiskWatcherBackgroundTaskQueue>();
			_processFile = processFile;
		}

		internal QueueProcessor(IDiskWatcherBackgroundTaskQueue diskWatcherBackgroundTaskQueue,
			SynchronizeDelegate processFile)
		{
			_bgTaskQueue = diskWatcherBackgroundTaskQueue;
			_processFile = processFile;
		}

		public delegate Task<List<FileIndexItem>> SynchronizeDelegate(Tuple<string, string, WatcherChangeTypes> value);


		public async Task QueueInput(string filepath, string toPath,
			WatcherChangeTypes changeTypes)
		{
			await _bgTaskQueue.QueueBackgroundWorkItemAsync(async _ =>
			{
				await _processFile.Invoke(new Tuple<string, string, WatcherChangeTypes>(filepath,toPath,changeTypes));
			}, $"from:{filepath}" + (string.IsNullOrEmpty(toPath) ? "" : "_to:" + toPath));
		}
	}

}
