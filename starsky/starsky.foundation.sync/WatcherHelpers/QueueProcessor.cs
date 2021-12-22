using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Models;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.sync.WatcherInterfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.sync.WatcherHelpers
{
	public class QueueProcessor : IQueueProcessor // not injected
	{
		private readonly IDiskWatcherBackgroundTaskQueue _bgTaskQueue;
		private readonly SynchronizeDelegate _processFile;
		private readonly IMemoryCache _memoryCache;
		private readonly TimeSpan _expirationTime = TimeSpan.FromSeconds(1);

		public QueueProcessor(IServiceScopeFactory serviceProvider,
			SynchronizeDelegate processFile, IMemoryCache memoryCache)
		{
			_bgTaskQueue = serviceProvider.CreateScope().ServiceProvider.GetService<IDiskWatcherBackgroundTaskQueue>();
			_processFile = processFile;
			_memoryCache = memoryCache;
		}
		
		public delegate Task<List<FileIndexItem>> SynchronizeDelegate(Tuple<string, string, WatcherChangeTypes> value);

		private static string CacheName(string filepath, string toPath)
		{
			return $"QueueProcessor{filepath}{toPath}";
		}

		public void QueueInput(string filepath, string toPath,  WatcherChangeTypes changeTypes)
		{
			// to avoid lots of events
			if (_memoryCache.TryGetValue(CacheName( filepath,  toPath), out _))
			{
				return;
			}
			_memoryCache.Set(CacheName( filepath,  toPath), 1, _expirationTime);
			// ends of avoid lots of events
			
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				await _processFile.Invoke(new Tuple<string, string, WatcherChangeTypes>(filepath,toPath,changeTypes));
			});
		}
	}

}
