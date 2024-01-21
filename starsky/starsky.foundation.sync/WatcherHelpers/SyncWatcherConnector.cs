#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.webtelemetry.Initializers;
using starsky.foundation.webtelemetry.Models;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.sync.WatcherHelpers
{
	public sealed class SyncWatcherConnector
	{
		private ISynchronize? _synchronize;
		private AppSettings? _appSettings;
		private IWebSocketConnectionsService? _connectionsService;
		private IQuery? _query;
		private IWebLogger? _logger;
		private readonly IServiceScope? _serviceScope;
		private TelemetryClient? _telemetryClient;
		private INotificationQuery? _notificationQuery;

		internal SyncWatcherConnector(AppSettings appSettings, ISynchronize synchronize, 
			IWebSocketConnectionsService connectionsService, IQuery query, IWebLogger logger, 
			INotificationQuery notificationQuery, TelemetryClient? telemetryClient)
		{
			_appSettings = appSettings;
			_synchronize = synchronize;
			_connectionsService = connectionsService;
			_query = query;
			_logger = logger;
			_notificationQuery = notificationQuery;
			_telemetryClient = telemetryClient;
		}

		public SyncWatcherConnector(IServiceScopeFactory scopeFactory)
		{
			_serviceScope = scopeFactory.CreateScope();
		}

		internal bool InjectScopes()
		{
			if ( _serviceScope == null ) return false;
			// ISynchronize is a scoped service
			_synchronize = _serviceScope.ServiceProvider.GetRequiredService<ISynchronize>();
			_appSettings = _serviceScope.ServiceProvider.GetRequiredService<AppSettings>();
			_connectionsService = _serviceScope.ServiceProvider.GetRequiredService<IWebSocketConnectionsService>();
			var query = _serviceScope.ServiceProvider.GetRequiredService<IQuery>();
			_logger = _serviceScope.ServiceProvider.GetRequiredService<IWebLogger>();
			var memoryCache = _serviceScope.ServiceProvider.GetService<IMemoryCache>();
			var serviceScopeFactory = _serviceScope.ServiceProvider.GetService<IServiceScopeFactory>();
			_query = new QueryFactory(new SetupDatabaseTypes(_appSettings), query,
				memoryCache, _appSettings, serviceScopeFactory, _logger).Query();
			_notificationQuery = _serviceScope.ServiceProvider
				.GetService<INotificationQuery>();
			_telemetryClient = _serviceScope.ServiceProvider
				.GetService<TelemetryClient>();
			return true;
		}

		internal IOperationHolder<RequestTelemetry> CreateNewRequestTelemetry(string? fullFilePath = null)
		{
			if (_telemetryClient == null || string.IsNullOrEmpty(_appSettings!
				    .ApplicationInsightsConnectionString) )
			{
				return new EmptyOperationHolder<RequestTelemetry>();
			}

			var requestTelemetry = new RequestTelemetry {Name = "FSW " + nameof(SyncWatcherConnector) };
			var operation = _telemetryClient.StartOperation(requestTelemetry);
			operation.Telemetry.Timestamp = DateTimeOffset.UtcNow;
			operation.Telemetry.Source = "FileSystem";
			if ( !string.IsNullOrEmpty(fullFilePath) )
			{
				operation.Telemetry.Url = new Uri($"?f={fullFilePath}", UriKind.Relative);
			}
			new CloudRoleNameInitializer($"{_appSettings.ApplicationType}").Initialize(requestTelemetry);
			return operation;
		}

		internal bool EndRequestOperation(IOperationHolder<RequestTelemetry> operation, string statusCode)
		{
			if ( _telemetryClient == null || string.IsNullOrEmpty(_appSettings!
				    .ApplicationInsightsConnectionString) )
			{
				return false;
			}
			
			// end operation
			operation.Telemetry.Success = true;
			operation.Telemetry.Duration = DateTimeOffset.UtcNow - operation.Telemetry.Timestamp;
			operation.Telemetry.ResponseCode = statusCode;
			_telemetryClient.StopOperation(operation);
			return true;
		}

		public Task<List<FileIndexItem>> Sync(
			Tuple<string, string, WatcherChangeTypes> watcherOutput)
		{
			// Avoid Disposed Query objects
			if ( _serviceScope != null ) InjectScopes();
			if ( _synchronize == null || _logger == null || _appSettings == null || _connectionsService == null || _query == null)
			{
				throw new ArgumentException("sync, logger, appSettings, _appSettings, _websockets or _query should not be null");
			}
			
			return SyncTaskInternal(watcherOutput);
		}

		/// <summary>
		/// Internal sync connector task
		/// </summary>
		/// <param name="watcherOutput">data</param>
		/// <returns>Task with data</returns>
		private async Task<List<FileIndexItem>> SyncTaskInternal(Tuple<string, string, WatcherChangeTypes> watcherOutput)
		{
			var (fullFilePath, toPath, type ) = watcherOutput;
			var operation = CreateNewRequestTelemetry(fullFilePath);

			var syncData = new List<FileIndexItem>();
			
			_logger!.LogInformation($"[SyncWatcherConnector] [{fullFilePath}] - [{toPath}] - [{type}]");
			
			if ( type == WatcherChangeTypes.Renamed && !string.IsNullOrEmpty(toPath))
			{
				// from path sync
				var path = _appSettings!.FullPathToDatabaseStyle(fullFilePath);
				await _synchronize!.Sync(path); 
				
				syncData.Add(new FileIndexItem(_appSettings.FullPathToDatabaseStyle(fullFilePath))
				{
					IsDirectory = true, 
					Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
				});
				
				// and now to-path sync
				var pathToDatabaseStyle = _appSettings.FullPathToDatabaseStyle(toPath);
				syncData.AddRange(await _synchronize.Sync(pathToDatabaseStyle));
			}
			else
			{
				syncData = await _synchronize!.Sync(_appSettings!.FullPathToDatabaseStyle(fullFilePath));
			}

			var filtered = FilterBefore(syncData);
			if ( filtered.Count == 0 )
			{
				_logger.LogInformation($"[SyncWatcherConnector/EndOperation] f:{filtered.Count}/s:{syncData.Count} ~ skip: "+ 
	                       string.Join(", ", syncData.Select(p => p.FileName).ToArray()) + " ~ " +
	                                         string.Join(", ", syncData.Select(p => p.Status).ToArray()));
				EndRequestOperation(operation, string.Join(", ", syncData.Select(p => p.Status).ToArray()));
				return syncData;
			}

			await PushToSockets(filtered);
			
			// And update the query Cache
			_query!.CacheUpdateItem(filtered.Where(p => p.Status == FileIndexItem.ExifStatus.Ok ||
				p.Status == FileIndexItem.ExifStatus.Deleted).ToList());
			
			// remove files that are not in the index from cache
			_query.RemoveCacheItem(filtered.Where(p => p.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex || 
				p.Status == FileIndexItem.ExifStatus.NotFoundSourceMissing).ToList());

			if ( _serviceScope != null ) await _query.DisposeAsync();
			EndRequestOperation(operation, "OK");
			
			return syncData;
		}

		/// <summary>
		/// Both websockets and NotificationAPI
		/// update users who are active right now
		/// </summary>
		/// <param name="filtered">list of messages to push</param>
		private async Task PushToSockets(List<FileIndexItem> filtered)
		{
			_logger!.LogInformation("[SyncWatcherConnector/Socket] "+ string.Join(", ", filtered.Select(p => p.FilePath).ToArray()));
			var webSocketResponse =
				new ApiNotificationResponseModel<List<FileIndexItem>>(filtered, ApiNotificationType.SyncWatcherConnector);
			await _connectionsService!.SendToAllAsync(JsonSerializer.Serialize(webSocketResponse,
				DefaultJsonSerializer.CamelCaseNoEnters), CancellationToken.None);
			await _notificationQuery!.AddNotification(webSocketResponse);
		}

		internal static List<FileIndexItem> FilterBefore(IReadOnlyCollection<FileIndexItem> syncData)
		{
			// also remove duplicates from output list
			return syncData.GroupBy(x => x.FilePath).
				Select(x => x.First())
				.Where(p =>
				p.Status is FileIndexItem.ExifStatus.Ok or 
					FileIndexItem.ExifStatus.Deleted or 
					FileIndexItem.ExifStatus.NotFoundNotInIndex or 
					FileIndexItem.ExifStatus.NotFoundSourceMissing).ToList();
		}
	}
}
