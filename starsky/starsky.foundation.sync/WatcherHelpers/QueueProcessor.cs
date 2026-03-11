using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Models;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.sync.WatcherInterfaces;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.sync.WatcherHelpers;

public sealed class QueueProcessor : IQueueProcessor // not injected
{
	public delegate Task<List<FileIndexItem>> SynchronizeDelegate(
		Tuple<string, string?, WatcherChangeTypes> value);

	public const string JobType = "Sync.QueueProcessorInput.v1";

	private readonly IDiskWatcherBackgroundTaskQueue _bgTaskQueue;
	private readonly SynchronizeDelegate _processFile;

	public QueueProcessor(IServiceScopeFactory serviceProvider,
		SynchronizeDelegate processFile)
	{
		_bgTaskQueue = serviceProvider.CreateScope().ServiceProvider
			.GetRequiredService<IDiskWatcherBackgroundTaskQueue>();
		_processFile = processFile;
	}

	internal QueueProcessor(IDiskWatcherBackgroundTaskQueue diskWatcherBackgroundTaskQueue,
		SynchronizeDelegate processFile)
	{
		_bgTaskQueue = diskWatcherBackgroundTaskQueue;
		_processFile = processFile;
	}


	public async Task QueueInput(string filepath, string? toPath,
		WatcherChangeTypes changeTypes)
	{
		var payload = new QueueProcessorPayload
		{
			FilePath = filepath, ToPath = toPath, ChangeTypes = changeTypes
		};
		await _bgTaskQueue.QueueJobAsync(new BackgroundTaskQueueJob
		{
			MetaData = $"from:{filepath}" +
			           ( string.IsNullOrEmpty(toPath) ? string.Empty : "_to:" + toPath ),
			TraceParentId = Activity.Current?.Id,
			PriorityLane = ProcessTaskQueue.PriorityLaneDiskWatcher,
			JobType = JobType,
			PayloadJson = JsonSerializer.Serialize(payload)
		});
	}
}

public sealed class QueueProcessorPayload
{
	public string FilePath { get; set; } = string.Empty;
	public string? ToPath { get; set; }
	public WatcherChangeTypes ChangeTypes { get; set; }
}
