using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
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
	private readonly IServiceScopeFactory? _scopeFactory;
	private readonly ITenantContext? _tenantContext;

	public QueueProcessor(IServiceScopeFactory serviceProvider)
	{
		_scopeFactory = serviceProvider;
		_bgTaskQueue = serviceProvider.CreateScope().ServiceProvider
			.GetRequiredService<IDiskWatcherBackgroundTaskQueue>();
		_tenantContext = null;
	}

	internal QueueProcessor(IDiskWatcherBackgroundTaskQueue diskWatcherBackgroundTaskQueue,
		ITenantContext? tenantContext = null)
	{
		_bgTaskQueue = diskWatcherBackgroundTaskQueue;
		_scopeFactory = null;
		_tenantContext = tenantContext;
	}


	public async Task QueueJob(string filepath, string? toPath,
		WatcherChangeTypes changeTypes)
	{
		var tenantId = _tenantContext?.TenantId;
		var tenantSlug = _tenantContext?.TenantSlug;

		if ( !tenantId.HasValue && string.IsNullOrWhiteSpace(tenantSlug) )
		{
			(tenantId, tenantSlug) = await ResolveTenantByFolderAsync(filepath, toPath);
		}

		var payload = new QueueProcessorPayload
		{
			FilePath = filepath,
			ToPath = toPath,
			ChangeTypes = changeTypes,
			TenantSlug = tenantSlug,
			TenantId = tenantId
		};
		await _bgTaskQueue.QueueJobAsync(new BackgroundTaskQueueJob
		{
			MetaData = $"from:{filepath}" +
			           ( string.IsNullOrEmpty(toPath) ? string.Empty : "_to:" + toPath ),
			TraceParentId = Activity.Current?.Id,
			TenantId = tenantId,
			TenantSlug = tenantSlug,
			PriorityLane = ProcessTaskQueue.PriorityLaneDiskWatcher,
			JobType = JobType,
			PayloadJson = JsonSerializer.Serialize(payload)
		});
	}

	private async Task<(int?, string?)> ResolveTenantByFolderAsync(string filepath, string? toPath)
	{
		if ( _scopeFactory == null )
		{
			return (null, null);
		}

		using var scope = _scopeFactory.CreateScope();
		var appSettings = scope.ServiceProvider.GetService<AppSettings>();
		var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
		if ( appSettings == null || dbContext == null )
		{
			return (null, null);
		}

		var folderSlug = ExtractTenantSlugFromPath(toPath ?? filepath, appSettings.StorageFolder);
		if ( string.IsNullOrWhiteSpace(folderSlug) )
		{
			return (null, null);
		}

		var tenant = await dbContext.Tenants
			.FirstOrDefaultAsync(t => t.Slug == folderSlug);
		return tenant == null ? (null, null) : (tenant.Id, tenant.Slug);
	}

	internal static string? ExtractTenantSlugFromPath(string fullPath, string storageFolder)
	{
		if ( string.IsNullOrWhiteSpace(fullPath) || string.IsNullOrWhiteSpace(storageFolder) )
		{
			return null;
		}

		var normalizedFullPath = fullPath.Replace('\\', '/');
		var normalizedStorage = storageFolder.Replace('\\', '/').TrimEnd('/');
		if ( !normalizedFullPath.StartsWith(normalizedStorage + "/", StringComparison.OrdinalIgnoreCase) )
		{
			return null;
		}

		var relativePath = normalizedFullPath[(normalizedStorage.Length + 1)..];
		var segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
		return segments.Length > 0 ? segments[0] : null;
	}
}

public sealed class QueueProcessorPayload
{
	public string FilePath { get; set; } = string.Empty;
	public string? ToPath { get; set; }
	public WatcherChangeTypes ChangeTypes { get; set; }
	public string? TenantSlug { get; set; }
	public int? TenantId { get; set; }
}
